using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;
using AcademicSystem.Events;

namespace PaymentService.Application.Commands;

public record CompletePaymentCommand(
    Guid PaymentId,
    string TransactionId,
    string? GatewayResponse = null
) : IRequest<Result<bool>>;

public class CompletePaymentCommandHandler : IRequestHandler<CompletePaymentCommand, Result<bool>>
{
    private readonly IPaymentRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CompletePaymentCommandHandler> _logger;

    public CompletePaymentCommandHandler(
        IPaymentRepository repository,
        IEventBus eventBus,
        ILogger<CompletePaymentCommandHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
                return Result<bool>.Failure($"Payment with ID {request.PaymentId} not found");
            
            payment.Complete(request.TransactionId, request.GatewayResponse);
            
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Publicar evento de pago completado
            await _eventBus.PublishAsync(new PaymentCompletedEvent(
                payment.Id,
                payment.StudentId,
                payment.Amount.Amount,
                request.TransactionId
            ));
            
            _logger.LogInformation("Payment completed: {PaymentId}, TransactionId: {TransactionId}", 
                request.PaymentId, request.TransactionId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing payment {PaymentId}", request.PaymentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}