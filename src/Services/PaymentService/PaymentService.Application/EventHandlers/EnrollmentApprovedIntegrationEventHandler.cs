using AcademicSystem.EventBus;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application.EventHandlers;

public class EnrollmentApprovedIntegrationEventHandler : IIntegrationEventHandler<EnrollmentApprovedEvent>
{
    private readonly ILogger<EnrollmentApprovedIntegrationEventHandler> _logger;

    public EnrollmentApprovedIntegrationEventHandler(ILogger<EnrollmentApprovedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(EnrollmentApprovedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PaymentService: Recibido EnrollmentApprovedEvent para EnrollmentId: {EnrollmentId}, StudentId: {StudentId}, CourseId: {CourseId}", 
            @event.EnrollmentId, @event.StudentId, @event.CourseId);
        
        // Aquí podrías agregar la lógica para crear un pago pendiente
        
        await Task.CompletedTask;
    }
}
