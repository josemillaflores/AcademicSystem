using MediatR;

namespace PaymentService.Application.Commands;

public record ProcessPaymentCommand(Guid PaymentId) : IRequest<Result<bool>>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<bool>>
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
                return Result<bool>.Failure($"Payment with ID {request.PaymentId} not found");
            
            payment.Process();
            
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Payment processed: {PaymentId}", request.PaymentId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", request.PaymentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}