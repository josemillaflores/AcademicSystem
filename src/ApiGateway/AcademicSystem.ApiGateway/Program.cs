using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using AcademicSystem.ApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AcademicSystem.ApiGateway")
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONFIGURACIÓN DE AUTENTICACIÓN JWT
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

// ==========================================
// 3. CONFIGURACIÓN DE RATE LIMITING
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
    };
    
    options.AddFixedWindowLimiter("fixed", fixedOptions =>
    {
        fixedOptions.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimit", 100);
        fixedOptions.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60));
        fixedOptions.QueueLimit = builder.Configuration.GetValue<int>("RateLimiting:QueueLimit", 10);
    });
});

// ==========================================
// 4. CONFIGURACIÓN DE CORS
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
// 5. CONFIGURACIÓN DE YARP
// ==========================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms =>
    {
        // Agregar headers personalizados
        transforms.AddRequestTransform(async transformContext =>
        {
            transformContext.ProxyRequest.Headers.Add("X-Forwarded-For", 
                transformContext.HttpContext.Connection.RemoteIpAddress?.ToString());
            transformContext.ProxyRequest.Headers.Add("X-Original-Host", 
                transformContext.HttpContext.Request.Host.Value);
            await ValueTask.CompletedTask;
        });
        
        transforms.AddResponseTransform(async transformContext =>
        {
            var logger = transformContext.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Proxy request: {Method} {Path} -> {StatusCode}", 
                transformContext.HttpContext.Request.Method,
                transformContext.HttpContext.Request.Path,
                transformContext.ProxyResponse?.StatusCode);
            await ValueTask.CompletedTask;
        });
    });

// ==========================================
// 6. CONFIGURACIÓN DE SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Academic System API Gateway",
        Version = "v1",
        Description = "API Gateway for Academic System Microservices using YARP",
        Contact = new OpenApiContact
        {
            Name = "Academic System Team",
            Email = "support@academic-system.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token"
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
// 7. CONFIGURACIÓN DE HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddCheck<ApiGatewayHealthCheck>("gateway_health", tags: new[] { "ready", "live" });

// ==========================================
// 8. CONFIGURACIÓN DE COMPRESIÓN
// ==========================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json", "text/json", "application/problem+json" };
});

var app = builder.Build();

// ==========================================
// 9. CONFIGURACIÓN DEL PIPELINE
// ==========================================

// Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health Checks endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Service = "API Gateway",
            Environment = app.Environment.EnvironmentName,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Endpoint de información del gateway
app.MapGet("/info", () => Results.Ok(new
{
    Name = "Academic System API Gateway",
    Version = "1.0.0",
    Framework = "YARP (Yet Another Reverse Proxy)",
    Timestamp = DateTime.UtcNow,
    Routes = app.Services.GetService<IProxyConfigProvider>()?.GetConfig().Routes.Count() ?? 0
}));

// Endpoint de rutas configuradas
app.MapGet("/routes", (IProxyConfigProvider proxyConfig) =>
{
    var config = proxyConfig.GetConfig();
    var routes = config.Routes.Select(r => new 
    { 
        r.RouteId, 
        Path = r.Match?.Path,
        ClusterId = r.ClusterId,
        Methods = r.Match?.Methods
    });
    var clusters = config.Clusters.Select(c => new
    {
        c.ClusterId,
        Destinations = c.Destinations?.Select(d => d.Value.Address) ?? Enumerable.Empty<string>()
    });
    
    return Results.Ok(new { routes, clusters });
});

// Mapear YARP Reverse Proxy
app.MapReverseProxy();

// ==========================================
// 10. INICIO DE LA APLICACIÓN
// ==========================================
Log.Information("Starting API Gateway");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

await app.RunAsync();

// ==========================================
// 11. IMPLEMENTACIÓN DE HEALTH CHECK
// ==========================================
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
            _logger.LogDebug("API Gateway health check executed at {Time}", DateTime.UtcNow);
            return Task.FromResult(HealthCheckResult.Healthy("API Gateway is running correctly"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("API Gateway is unhealthy", ex));
        }
    }
}