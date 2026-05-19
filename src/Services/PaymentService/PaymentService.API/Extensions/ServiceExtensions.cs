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
using PaymentService.Application.Commands;
using PaymentService.Application.Mappings;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using AcademicSystem.EventBus;
using PaymentService.Application.EventHandlers;

namespace PaymentService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddPaymentServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePaymentCommandHandler).Assembly));
        services.AddAutoMapper(typeof(PaymentProfile));

        // Base de Datos & Repositorios
        services.AddDbContext<PaymentDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // EventBus & RabbitMQ
        services.AddSingleton<IEventBus, EventBusRabbitMQ>();
        services.AddTransient<EnrollmentApprovedIntegrationEventHandler>();

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
        services.AddHealthChecks().AddDbContextCheck<PaymentDbContext>("database");

        return services;
    }

    public static async Task ConfigureEventSubscriptionsAsync(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        await eventBus.SubscribeAsync<EnrollmentApprovedEvent, EnrollmentApprovedIntegrationEventHandler>();
    }

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
