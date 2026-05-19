using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace AcademicSystem.ApiGateway.Extensions;

public static class ServiceExtensions
{
    public static void AddApiLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "AcademicSystem.ApiGateway")
            .WriteTo.Console()
            .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authConfig = configuration.GetSection("Auth");
        var secretKey = Encoding.UTF8.GetBytes(authConfig["SecretKey"] ?? "default-secret-key-32-chars-long-minimum!");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiGatewayHealthCheck>>();
                        logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiGatewayHealthCheck>>();
                        logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("academic-api-policy", policy =>
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

        return services;
    }

    public static IServiceCollection AddApiRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
            };
            
            options.AddFixedWindowLimiter("fixed", fixedOptions =>
            {
                fixedOptions.PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 100);
                fixedOptions.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:WindowInSeconds", 60));
                fixedOptions.QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 10);
            });
        });

        return services;
    }

    public static IServiceCollection AddApiCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddApiYarp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(transforms =>
            {
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
                    var logger = transformContext.HttpContext.RequestServices.GetRequiredService<ILogger<ApiGatewayHealthCheck>>();
                    logger.LogInformation("Proxy request: {Method} {Path} -> {StatusCode}", 
                        transformContext.HttpContext.Request.Method,
                        transformContext.HttpContext.Request.Path,
                        transformContext.ProxyResponse?.StatusCode);
                    await ValueTask.CompletedTask;
                });
            });

        return services;
    }

    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
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

        return services;
    }

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ApiGatewayHealthCheck>("gateway_health", tags: new[] { "ready", "live" });

        return services;
    }

    public static IServiceCollection AddApiResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = new[] { "application/json", "text/json", "application/problem+json" };
        });

        return services;
    }
}

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
