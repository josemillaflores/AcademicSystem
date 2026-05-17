using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Commands;

public record RefundPaymentCommand(
    Guid PaymentId,
    decimal? Amount = null
) : IRequest<Result<Guid>>;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<Guid>>
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository repository,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
            
            if (payment == null)
                return Result<Guid>.Failure($"Payment with ID {request.PaymentId} not found");
            
            var refundAmount = request.Amount ?? payment.Amount.Amount;
            var refund = payment.Refund(refundAmount);
            
            await _repository.UpdateAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Payment refunded: {PaymentId}, Amount: {Amount}", 
                request.PaymentId, refundAmount);
            return Result<Guid>.Success(refund.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", request.PaymentId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}