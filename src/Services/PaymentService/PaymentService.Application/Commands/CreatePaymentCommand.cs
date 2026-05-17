using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Commands;

public record CreatePaymentCommand(
    Guid StudentId,
    decimal Amount,
    string Method,
    string Currency = "USD"
) : IRequest<Result<Guid>>;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<Guid>>
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IPaymentRepository repository,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paymentNumber = GeneratePaymentNumber();
            var paymentMethod = Enum.Parse<PaymentMethod>(request.Method);
            
            var payment = new Payment(
                request.StudentId,
                request.Amount,
                paymentMethod,
                paymentNumber,
                request.Currency
            );
            
            await _repository.AddAsync(payment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Payment created with ID: {PaymentId}, Number: {PaymentNumber}", 
                payment.Id, paymentNumber);
            
            return Result<Guid>.Success(payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return Result<Guid>.Failure(ex.Message);
        }
    }
    
    private string GeneratePaymentNumber()
    {
        return $"PAY-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
    }
}