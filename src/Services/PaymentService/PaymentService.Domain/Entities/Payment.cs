using AcademicSystem.Common.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public class Payment : BaseEntity
{
    private readonly List<Transaction> _transactions = new();

    public string PaymentNumber { get; private set; } = null!;
    public Guid StudentId { get; private set; }
    public string StudentName { get; private set; } = null!;
    public string StudentNumber { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
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

    public void Process()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment with status {Status}");

        Status = PaymentStatus.Processing;
        UpdateTimestamp();
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

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail payment with status {Status}");

        Status = PaymentStatus.Failed;
        GatewayResponse = reason;
        UpdateTimestamp();
    }

    public Transaction Refund(decimal amount)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Cannot refund payment with status {Status}");

        Status = PaymentStatus.Refunded;
        UpdateTimestamp();
        
        var transactionId = $"REF-{Guid.NewGuid().ToString().Substring(0, 8)}";
        var transaction = new Transaction(transactionId, -amount, "Refunded");
        _transactions.Add(transaction);
        
        return transaction;
    }
}