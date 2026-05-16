using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TeacherService.Application.Commands;
using TeacherService.Domain.Interfaces;
using TeacherService.Infrastructure.Data;
using TeacherService.Infrastructure.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "TeacherService")
    .WriteTo.Console()
    .WriteTo.File("logs/teacher-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONFIGURACIÓN DE CONTROLADORES
// ==========================================
builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.RegisterValidatorsFromAssemblyContaining<CreateTeacherCommandValidator>();
        v.ImplicitlyValidateChildProperties = true;
    });

// ==========================================
// 3. SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teacher Service API",
        Version = "v1",
        Description = "Microservicio para gestión de docentes"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
});

// ==========================================
// 4. AUTENTICACIÓN JWT
// ==========================================
var authConfig = builder.Configuration.GetSection("Auth");
var secretKey = Encoding.UTF8.GetBytes(authConfig["SecretKey"] ?? "default-secret-key-32-chars");

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
    cfg.RegisterServicesFromAssembly(typeof(CreateTeacherCommandHandler).Assembly);
});

// ==========================================
// 6. AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(typeof(TeacherProfile));

// ==========================================
// 7. BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<TeacherDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ==========================================
// 8. REPOSITORIOS
// ==========================================
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
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
        fixedOptions.PermitLimit = 100;
        fixedOptions.Window = TimeSpan.FromSeconds(60);
        fixedOptions.QueueLimit = 10;
    });
});

// ==========================================
// 11. HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TeacherDbContext>("database");

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
    var dbContext = scope.ServiceProvider.GetRequiredService<TeacherDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ==========================================
// 15. INICIO
// ==========================================
Log.Information("Starting Teacher Service API");
await app.RunAsync();