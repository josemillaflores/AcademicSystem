using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StudentService.Application.Commands;
using StudentService.Application.Validators;
using StudentService.Domain.Interfaces;
using StudentService.Infrastructure.Data;
using StudentService.Infrastructure.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "StudentService")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/student-service-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONFIGURACIÓN DE CONTROLADORES
// ==========================================
builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.RegisterValidatorsFromAssemblyContaining<CreateStudentCommandValidator>();
        v.ImplicitlyValidateChildProperties = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return new BadRequestObjectResult(new
            {
                Title = "Validation Error",
                Status = 400,
                Errors = errors,
                TraceId = context.HttpContext.TraceIdentifier
            });
        };
    });

// ==========================================
// 3. CONFIGURACIÓN DE SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Service API",
        Version = "v1",
        Description = "Microservicio para gestión de estudiantes",
        Contact = new OpenApiContact
        {
            Name = "Academic System Team",
            Email = "support@academicsystem.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de su token JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ==========================================
// 4. CONFIGURACIÓN DE AUTENTICACIÓN JWT
// ==========================================
var authConfig = builder.Configuration.GetSection("Auth");
var secretKey = Encoding.UTF8.GetBytes(authConfig["SecretKey"] ?? "default-secret-key-32-chars-long-minimum!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authConfig["Issuer"],
            ValidAudience = authConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "academic_api");
    });
});

// ==========================================
// 5. CONFIGURACIÓN DE MEDIATR (CQRS)
// ==========================================
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateStudentCommandHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// ==========================================
// 6. CONFIGURACIÓN DE AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(typeof(StudentProfile));

// ==========================================
// 7. CONFIGURACIÓN DE BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<StudentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// ==========================================
// 8. CONFIGURACIÓN DE REPOSITORIOS
// ==========================================
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==========================================
// 9. CONFIGURACIÓN DE CORS
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://academic-system.com", "https://admin.academic-system.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================================
// 10. CONFIGURACIÓN DE RATE LIMITING
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", fixedOptions =>
    {
        fixedOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100);
        fixedOptions.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60));
        fixedOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10);
    });
});

// ==========================================
// 11. CONFIGURACIÓN DE HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StudentDbContext>("database", tags: new[] { "ready", "live" })
    .AddCheck<ApiHealthCheck>("api", tags: new[] { "live" });

// ==========================================
// 12. CONFIGURACIÓN DE COMPRESIÓN
// ==========================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json", "text/json" };
});

// ==========================================
// 13. CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================
var app = builder.Build();

// ==========================================
// 14. CONFIGURACIÓN DEL PIPELINE
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseHsts();
    app.UseCors("Production");
}

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});

app.MapControllers();

// ==========================================
// 15. APLICAR MIGRACIONES
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// ==========================================
// 16. INICIO
// ==========================================
Log.Information("Starting Student Service API");
await app.RunAsync();

// ==========================================
// 17. CLASES ADICIONALES
// ==========================================

public class ApiHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiHealthCheck> _logger;
    
    public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
    {
        _logger = logger;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(HealthCheckResult.Healthy("API is healthy"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("API is unhealthy", ex));
        }
    }
}

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
            
            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        return await next();
    }
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}