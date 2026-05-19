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
using EnrollmentService.Application.Commands;
using EnrollmentService.Application.Mappings;
using EnrollmentService.Application.Services;
using EnrollmentService.Domain.Interfaces;
using EnrollmentService.Infrastructure;
using EnrollmentService.Infrastructure.Data;
using EnrollmentService.Infrastructure.Repositories;
using AcademicSystem.EventBus;
using EnrollmentService.Application.EventHandlers;
using AcademicSystem.Common.Extensions;

namespace EnrollmentService.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddEnrollmentServices(this IServiceCollection services, IConfiguration configuration)
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

        // MediatR & AutoMapper & Services
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApproveEnrollmentCommandHandler).Assembly));
        services.AddAutoMapper(typeof(EnrollmentProfile));
        services.AddHttpContextAccessor();

        // Base de Datos & Repositorios
        services.AddDbContext<EnrollmentDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEnrollmentCompositionService, EnrollmentCompositionService>();

        // EventBus & RabbitMQ
        services.AddSingleton<IEventBus, EventBusRabbitMQ>();
        services.AddTransient<PaymentCompletedIntegrationEventHandler>();

        // HttpClients con Polly Resilience Policies
        services.AddHttpClient("StudentService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:StudentService"] ?? "http://studentservice:8080");
        })
        .AddResiliencePolicies();

        services.AddHttpClient("CourseService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:CourseService"] ?? "http://courseservice:8080");
        })
        .AddResiliencePolicies();

        services.AddHttpClient("PaymentService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:PaymentService"] ?? "http://paymentservice:8080");
        })
        .AddResiliencePolicies();

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
        services.AddHealthChecks().AddDbContextCheck<EnrollmentDbContext>("database");

        return services;
    }

    public static async Task ConfigureEventSubscriptionsAsync(this WebApplication app)
    {
        var eventBus = app.Services.GetRequiredService<IEventBus>();
        await eventBus.SubscribeAsync<PaymentCompletedEvent, PaymentCompletedIntegrationEventHandler>();
    }

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
