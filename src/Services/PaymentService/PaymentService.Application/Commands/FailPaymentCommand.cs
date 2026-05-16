using MediatR;

namespace PaymentService.Application.Commands;

public record FailPaymentCommand(
    Guid PaymentId,
    string Reason
) : IRequest<Result<bool>>;

public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand, Result<bool>>
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<FailPaymentCommandHandler> _logger;

    public FailPaymentCommandHandler(
        IPaymentRepository repository,
        ILogger<FailPaymentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
                return Result<bool>.Failure($"Payment with ID {request.PaymentId} not found");
            
            payment.Fail(request.Reason);
            
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Payment failed: {PaymentId}, Reason: {Reason}", 
                request.PaymentId, request.Reason);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing payment {PaymentId}", request.PaymentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}