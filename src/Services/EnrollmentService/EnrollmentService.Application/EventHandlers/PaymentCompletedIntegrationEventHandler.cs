using AcademicSystem.EventBus;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EnrollmentService.Application.EventHandlers;

public class PaymentCompletedIntegrationEventHandler : IIntegrationEventHandler<PaymentCompletedEvent>
{
    private readonly ILogger<PaymentCompletedIntegrationEventHandler> _logger;

    public PaymentCompletedIntegrationEventHandler(ILogger<PaymentCompletedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("EnrollmentService: Recibido PaymentCompletedEvent para PaymentId: {PaymentId}, StudentId: {StudentId}, Amount: {Amount}, TransactionId: {TransactionId}", 
            @event.PaymentId, @event.StudentId, @event.Amount, @event.TransactionId);
        
        // Aquí podrías agregar la lógica para completar la matrícula o confirmar el pago.
        
        await Task.CompletedTask;
    }
}
