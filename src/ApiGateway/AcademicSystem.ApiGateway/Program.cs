using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AcademicSystem.ApiGateway")
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar JWT Authentication
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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "academic_api");
    });
    
    options.AddPolicy("admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });
});

// Configurar Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60))
            }));
    
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
    };
});

// Configurar CORS
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
              .AllowAnyHeader()
              .WithExposedHeaders("X-Total-Count", "X-Page-Number");
    });
});

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<ApiGatewayHealthCheck>("gateway_health")
    .AddUrlGroup(new Uri("http://studentservice:8080/health"), "student_service")
    .AddUrlGroup(new Uri("http://teacherservice:8080/health"), "teacher_service")
    .AddUrlGroup(new Uri("http://courseservice:8080/health"), "course_service")
    .AddUrlGroup(new Uri("http://enrollmentservice:8080/health"), "enrollment_service")
    .AddUrlGroup(new Uri("http://paymentservice:8080/health"), "payment_service");

// Configurar YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomTransformProvider>();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Academic System API Gateway",
        Version = "v1",
        Description = "API Gateway for Academic System Microservices",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Academic System Team",
            Email = "support@academic-system.com"
        }
    });
    
    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json", "text/json" };
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configurar Health Checks endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Services = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Endpoint de configuración de rutas (para debug)
app.MapGet("/routes", (IProxyConfigProvider proxyConfig) =>
{
    var config = proxyConfig.GetConfig();
    return Results.Ok(new
    {
        Routes = config.Routes.Select(r => new { r.RouteId, r.Match?.Path }),
        Clusters = config.Clusters.Keys
    });
}).WithTags("Debug");

// Endpoint de información del gateway
app.MapGet("/info", () => Results.Ok(new
{
    Name = "Academic System API Gateway",
    Version = "1.0.0",
    Framework = "YARP",
    Timestamp = DateTime.UtcNow
})).WithTags("Info");

// Agregar después de app.UseAuthorization()
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>()
// Mapear YARP Reverse Proxy
app.MapReverseProxy();

await app.RunAsync();

// Health Check Implementation
public class ApiGatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiGatewayHealthCheck> _logger;
    
    public ApiGatewayHealthCheck(ILogger<ApiGatewayHealthCheck> logger)
    {
        _logger = logger;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("API Gateway health check executed at {Time}", DateTime.UtcNow);
            return Task.FromResult(HealthCheckResult.Healthy("API Gateway is running correctly"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("API Gateway is unhealthy", ex));
        }
    }
}

// Custom Transform Provider for YARP
public class CustomTransformProvider : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
        // Agregar headers personalizados
        context.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Add("X-Forwarded-For", 
                transformContext.HttpContext.Connection.RemoteIpAddress?.ToString());
            transformContext.ProxyRequest.Headers.Add("X-Original-Host", 
                transformContext.HttpContext.Request.Host.Value);
            return ValueTask.CompletedTask;
        });
        
        // Agregar logging de requests
        context.AddResponseTransform(transformContext =>
        {
            var logger = transformContext.HttpContext.RequestServices.GetRequiredService<ILogger<CustomTransformProvider>>();
            logger.LogInformation("Proxy request: {Method} {Path} -> {StatusCode}", 
                transformContext.HttpContext.Request.Method,
                transformContext.HttpContext.Request.Path,
                transformContext.ProxyResponse?.StatusCode);
            return ValueTask.CompletedTask;
        });
    }
    
    public void ValidateRoute(TransformRouteValidationContext context) { }
    public void ValidateCluster(TransformClusterValidationContext context) { }
}