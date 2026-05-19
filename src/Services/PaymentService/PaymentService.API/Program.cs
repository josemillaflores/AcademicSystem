using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using AcademicSystem.Common.Middleware;
using PaymentService.API.Extensions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Registrar todos los servicios mediante método de extensión
builder.Services.AddPaymentServices(builder.Configuration);

var app = builder.Build();

// Configuración del pipeline de solicitudes
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiter();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

// Aplicar migraciones y suscribir eventos del bus automáticamente
await app.RunMigrationsAsync();
await app.ConfigureEventSubscriptionsAsync();

await app.RunAsync();
