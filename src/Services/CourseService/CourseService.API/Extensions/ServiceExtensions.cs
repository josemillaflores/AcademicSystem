using System;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using CourseService.Application.Commands;
using CourseService.Application.Mappings;
using CourseService.Domain.Interfaces;
using CourseService.Infrastructure.Data;
using CourseService.Infrastructure.Repositories;

namespace CourseService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCourseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Autenticación y Autorización
        var key = Encoding.UTF8.GetBytes("your-super-secret-key-with-at-least-32-characters-long");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        // MediatR & AutoMapper
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(CreateCourseCommandHandler).Assembly));
        services.AddAutoMapper(typeof(CourseProfile));

        // Base de Datos & Repositorios
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=CourseDb;Username=postgres;Password=Admin123!";
        services.AddDbContext<CourseDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // CORS & Rate Limiter
        services.AddCors(options => 
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        
        services.AddRateLimiter(options => 
            options.AddFixedWindowLimiter("fixed", opt => 
            { 
                opt.PermitLimit = 100; 
                opt.Window = TimeSpan.FromSeconds(60); 
                opt.QueueLimit = 10;
            }));

        // Health Checks
        services.AddHealthChecks().AddDbContextCheck<CourseDbContext>("database");

        return services;
    }

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
