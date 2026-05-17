using AcademicSystem.Common.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public class Payment : BaseEntity
{
    private readonly List<Transaction> _transactions = new();

    public string PaymentNumber { get; private set; }
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; }
    public string StudentNumber { get; private set; }
    public Money Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayResponse { get; private set; }
    
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Payment() { }

    public Payment(Guid studentId, decimal amount, PaymentMethod method, string paymentNumber, string currency = "USD")
    {
        StudentId = studentId;
        Amount = new Money(amount, currency);
        Method = method;
        PaymentNumber = paymentNumber;
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Pending;
    }

    public void Complete(string transactionId, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment with status {Status}");

        Status = PaymentStatus.Completed;
        GatewayResponse = gatewayResponse;
        
        var transaction = new Transaction(transactionId, Amount.Amount, gatewayResponse);
        _transactions.Add(transaction);
        
        UpdateTimestamp();
    }
}