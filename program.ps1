# Script para crear UnitOfWork en cada servicio y actualizar Program.cs
$services = @(
    @{Name="StudentService"; CommandHandler="CreateStudentCommandHandler"; Profile="StudentProfile"; DbContext="StudentDbContext"; Repository="IStudentRepository"; RepoImpl="StudentRepository"},
    @{Name="TeacherService"; CommandHandler="CreateTeacherCommandHandler"; Profile="TeacherProfile"; DbContext="TeacherDbContext"; Repository="ITeacherRepository"; RepoImpl="TeacherRepository"},
    @{Name="CourseService"; CommandHandler="CreateCourseCommandHandler"; Profile="CourseProfile"; DbContext="CourseDbContext"; Repository="ICourseRepository"; RepoImpl="CourseRepository"},
    @{Name="EnrollmentService"; CommandHandler="CreateEnrollmentCommandHandler"; Profile="EnrollmentProfile"; DbContext="EnrollmentDbContext"; Repository="IEnrollmentRepository"; RepoImpl="EnrollmentRepository"},
    @{Name="PaymentService"; CommandHandler="CreatePaymentCommandHandler"; Profile="PaymentProfile"; DbContext="PaymentDbContext"; Repository="IPaymentRepository"; RepoImpl="PaymentRepository"}
)

$basePath = "D:\jose\MicroserviciosNet10\Mayo2026\AcademicSystem\src\Services"

foreach ($service in $services) {
    $name = $service.Name
    $infraPath = "$basePath\$name\$name.Infrastructure\Repositories"
    
    # Crear carpeta si no existe
    if (-not (Test-Path $infraPath)) {
        New-Item -Path $infraPath -ItemType Directory -Force
    }
    
    # Crear UnitOfWork.cs
    $unitOfWorkContent = @"
using $name.Domain.Interfaces;
using $name.Infrastructure.Data;

namespace $name.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly $($service.DbContext) _context;
    private bool _disposed;

    public UnitOfWork($($service.DbContext) context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
"@
    Set-Content -Path "$infraPath\UnitOfWork.cs" -Value $unitOfWorkContent -Force
    Write-Host "Creado UnitOfWork para $name" -ForegroundColor Green
    
    # Actualizar Program.cs simplificado
    $programPath = "$basePath\$name\$name.API\Program.cs"
    
    $programContent = @"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using $name.Application.Commands;
using $name.Application.Mappings;
using $name.Domain.Interfaces;
using $name.Infrastructure.Data;
using $name.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.UTF8.GetBytes("your-super-secret-key-32-chars-minimum!");
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

builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof($($service.CommandHandler)).Assembly));
builder.Services.AddAutoMapper(typeof($($service.Profile)));
builder.Services.AddDbContext<$($service.DbContext)>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<$($service.Repository), $($service.RepoImpl)>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("fixed", opt => { opt.PermitLimit = 100; opt.Window = TimeSpan.FromSeconds(60); }));
builder.Services.AddHealthChecks().AddDbContextCheck<$($service.DbContext)>("database");

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
    var dbContext = scope.ServiceProvider.GetRequiredService<$($service.DbContext)>();
    await dbContext.Database.EnsureCreatedAsync();
}

await app.RunAsync();
"@
    Set-Content -Path $programPath -Value $programContent -Force
    Write-Host "Actualizado Program.cs para $name" -ForegroundColor Green
}

Write-Host "Todos los servicios actualizados correctamente" -ForegroundColor Green