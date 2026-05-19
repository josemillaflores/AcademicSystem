using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using AcademicSystem.Common.Middleware;
using AcademicSystem.ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE REGISTROS DE SERVICIOS
// ==========================================
builder.AddApiLogging();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiRateLimiter(builder.Configuration);
builder.Services.AddApiCors();
builder.Services.AddApiYarp(builder.Configuration);
builder.Services.AddApiSwagger();
builder.Services.AddApiHealthChecks();
builder.Services.AddApiResponseCompression();

var app = builder.Build();

// ==========================================
// 2. CONFIGURACIÓN DEL PIPELINE DE SOLICITUD
// ==========================================

// Middlewares comunes
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

// Endpoint para generar un token de desarrollo
app.MapGet("/token", () =>
{
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Auth:SecretKey"] ?? "default-secret-key-32-chars-long-minimum!");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim("sub", "dev-user"),
            new System.Security.Claims.Claim("scope", "academic_api"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
        }),
        Expires = DateTime.UtcNow.AddHours(24),
        Issuer = builder.Configuration["Auth:Issuer"] ?? "academic_auth",
        Audience = builder.Configuration["Auth:Audience"] ?? "academic_api",
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);
    return Results.Ok(new { token = tokenString, type = "Bearer" });
});

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
// 3. INICIO DE LA APLICACIÓN
// ==========================================
Log.Information("Starting API Gateway");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

await app.RunAsync();