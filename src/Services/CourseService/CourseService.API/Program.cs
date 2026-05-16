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
using CourseService.Application.Commands;
using CourseService.Domain.Interfaces;
using CourseService.Infrastructure.Data;
using CourseService.Infrastructure.Repositories;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERILOG
// ==========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "CourseService")
    .WriteTo.Console()
    .WriteTo.File("logs/course-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. CONTROLADORES
// ==========================================
builder.Services.AddControllers()
    .AddFluentValidation(v =>
    {
        v.RegisterValidatorsFromAssemblyContaining<CreateCourseCommandValidator>();
    });

// ==========================================
// 3. SWAGGER
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Course Service API",
        Version = "v1",
        Description = "Microservicio para gestión de cursos"
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
    cfg.RegisterServicesFromAssembly(typeof(CreateCourseCommandHandler).Assembly);
});

// ==========================================
// 6. AUTOMAPPER
// ==========================================
builder.Services.AddAutoMapper(typeof(CourseProfile));

// ==========================================
// 7. BASE DE DATOS
// ==========================================
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ==========================================
// 8. REPOSITORIOS
// ==========================================
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
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
    });
});

// ==========================================
// 11. HEALTH CHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CourseDbContext>("database");

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
    var dbContext = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ==========================================
// 15. INICIO
// ==========================================
Log.Information("Starting Course Service API");
await app.RunAsync();