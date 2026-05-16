using System.Reflection;
using System.Text;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using EnrollmentService.Application.Commands;
using EnrollmentService.Application.Services;
using EnrollmentService.Domain.Interfaces;
using EnrollmentService.Infrastructure.Data;
using EnrollmentService.Infrastructure.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "EnrollmentService")
    .WriteTo.Console()
    .WriteTo.File("logs/enrollment-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONTROLADORES
// ==========================================
builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.RegisterValidatorsFromAssemblyContaining<CreateEnrollmentCommandValidator>();
    });

// ==========================================
// 3. SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enrollment Service API",
        Version = "v1",
        Description = "Microservicio para gestión de matrículas"
    });
});

// ==========================================
// 4. JWT
// ==========================================
var authConfig = builder.Configuration.GetSection("Auth");
var secretKey = Encoding.UTF8.GetBytes(authConfig["SecretKey"] ?? "default-secret-key");

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

builder.Services.AddAuthorization();

// ==========================================
// 5. MEDIATR
// ==========================================
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateEnrollmentCommandHandler).Assembly);
});

// ==========================================
// 6. AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(typeof(EnrollmentProfile));

// ==========================================
// 7. BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<EnrollmentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ==========================================
// 8. REPOSITORIOS
// ==========================================
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==========================================
// 9. SERVICIO DE COMPOSICIÓN (API COMPOSITION)
// ==========================================
builder.Services.AddScoped<IEnrollmentCompositionService, EnrollmentCompositionService>();

// ==========================================
// 10. HTTP CLIENTS CON RESILIENCIA
// ==========================================

// StudentService HttpClient
builder.Services.AddHttpClient("StudentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:StudentService"] ?? "http://studentservice:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// CourseService HttpClient
builder.Services.AddHttpClient("CourseService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CourseService"] ?? "http://courseservice:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// PaymentService HttpClient
builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService"] ?? "http://paymentservice:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy());

// ==========================================
// 11. POLÍTICAS DE RESILIENCIA
// ==========================================
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

// ==========================================
// 12. CORS
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================================
// 13. RATE LIMITING
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", fixedOptions =>
    {
        fixedOptions.PermitLimit = 100;
        fixedOptions.Window = TimeSpan.FromSeconds(60);
    });
});

// ==========================================
// 14. HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EnrollmentDbContext>("database")
    .AddUrlGroup(new Uri(builder.Configuration["Services:StudentService"] ?? "http://studentservice:8080/health"), "student-service")
    .AddUrlGroup(new Uri(builder.Configuration["Services:CourseService"] ?? "http://courseservice:8080/health"), "course-service");

// ==========================================
// 15. CONSTRUCCIÓN
// ==========================================
var app = builder.Build();

// ==========================================
// 16. PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

// ==========================================
// 17. MIGRACIONES
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ==========================================
// 18. INICIO
// ==========================================
Log.Information("Starting Enrollment Service API");
await app.RunAsync();