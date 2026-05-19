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
using StudentService.Application.Commands;
using StudentService.Application.Mappings;
using StudentService.Domain.Interfaces;
using StudentService.Infrastructure.Data;
using StudentService.Infrastructure.Repositories;

namespace StudentService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddStudentServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateStudentCommandHandler).Assembly));
        services.AddAutoMapper(typeof(StudentProfile));

        // Base de Datos & Repositorios
        services.AddDbContext<StudentDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // CORS & Rate Limiter
        services.AddCors(options => 
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        
        services.AddRateLimiter(options => 
            options.AddFixedWindowLimiter("fixed", opt => 
            { 
                opt.PermitLimit = 100; 
                opt.Window = TimeSpan.FromSeconds(60); 
            }));

        // Health Checks
        services.AddHealthChecks().AddDbContextCheck<StudentDbContext>("database");

        return services;
    }

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
