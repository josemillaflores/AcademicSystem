using System.Reflection;
using System.Text;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using PaymentService.Application.Commands;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "PaymentService")
    .WriteTo.Console()
    .WriteTo.File("logs/payment-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONTROLADORES
// ==========================================
builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.RegisterValidatorsFromAssemblyContaining<CreatePaymentCommandValidator>();
    });

// ==========================================
// 3. SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "Microservicio para gestión de pagos"
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
    cfg.RegisterServicesFromAssembly(typeof(CreatePaymentCommandHandler).Assembly);
});

// ==========================================
// 6. AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(typeof(PaymentProfile));

// ==========================================
// 7. BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ==========================================
// 8. REPOSITORIOS
// ==========================================
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==========================================
// 9. CORS
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
// 10. RATE LIMITING
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", fixedOptions =>
    {
        fixedOptions.PermitLimit = 50; // Pagos tienen límite más bajo
        fixedOptions.Window = TimeSpan.FromSeconds(60);
    });
});

// ==========================================
// 11. HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentDbContext>("database");

// ==========================================
// 12. CONSTRUCCIÓN
// ==========================================
var app = builder.Build();

// ==========================================
// 13. PIPELINE
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
// 14. MIGRACIONES
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ==========================================
// 15. INICIO
// ==========================================
Log.Information("Starting Payment Service API");
await app.RunAsync();