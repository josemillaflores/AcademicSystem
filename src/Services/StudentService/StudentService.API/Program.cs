using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using StudentService.Application.Commands;
using StudentService.Application.Mappings;
using StudentService.Domain.Interfaces;
using StudentService.Infrastructure.Data;
using StudentService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.UTF8.GetBytes("default-secret-key-32-chars-minimum!");
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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateStudentServiceCommandHandler).Assembly));
builder.Services.AddAutoMapper(typeof(StudentServiceProfile));
builder.Services.AddDbContext<StudentServiceDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IStudentServiceRepository, StudentServiceRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("fixed", opt => { opt.PermitLimit = 100; opt.Window = TimeSpan.FromSeconds(60); }));
builder.Services.AddHealthChecks().AddDbContextCheck<StudentServiceDbContext>("database");

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

await app.RunAsync();
