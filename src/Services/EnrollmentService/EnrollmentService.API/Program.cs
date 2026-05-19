using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using EnrollmentService.Application.Commands;
using EnrollmentService.Application.Mappings;
using EnrollmentService.Application.Services;
using EnrollmentService.Domain.Interfaces;
using EnrollmentService.Infrastructure;
using EnrollmentService.Infrastructure.Data;
using EnrollmentService.Infrastructure.Repositories;
using Polly;
using Polly.Extensions.Http;
using AcademicSystem.EventBus;
using EnrollmentService.Application.EventHandlers;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.UTF8.GetBytes("your-super-secret-key-with-at-least-32-characters-long");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApproveEnrollmentCommandHandler).Assembly));
builder.Services.AddAutoMapper(typeof(EnrollmentProfile));
builder.Services.AddDbContext<EnrollmentDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEnrollmentCompositionService, EnrollmentCompositionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IEventBus, EventBusRabbitMQ>();
builder.Services.AddTransient<PaymentCompletedIntegrationEventHandler>();

builder.Services.AddHttpClient("StudentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:StudentService"] ?? "http://studentservice:8080");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("CourseService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CourseService"] ?? "http://courseservice:8080");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentService"] ?? "http://paymentservice:8080");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("fixed", opt => { opt.PermitLimit = 100; opt.Window = TimeSpan.FromSeconds(60); }));
builder.Services.AddHealthChecks().AddDbContextCheck<EnrollmentDbContext>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnrollmentDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.SubscribeAsync<PaymentCompletedEvent, PaymentCompletedIntegrationEventHandler>();

await app.RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
