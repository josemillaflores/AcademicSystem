using PaymentService.Domain.ValueObjects;
using PaymentService.Domain.Enums;

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
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateStudentInfo(string name, string number)
    {
        StudentName = name;
        StudentNumber = number;
        UpdateTimestamp();
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
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed payment");

        Status = PaymentStatus.Failed;
        GatewayResponse = reason;
        UpdateTimestamp();
    }

    public Transaction Refund(decimal amount)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot refund a payment that is not completed");

        if (amount > Amount.Amount)
            throw new InvalidOperationException("Refund amount cannot exceed payment amount");

        var refundTransaction = new Transaction($"REF-{PaymentNumber}", amount, "Refund processed");
        _transactions.Add(refundTransaction);
        
        UpdateTimestamp();
        return refundTransaction;
    }

    public decimal GetTotalPaid()
    {
        return _transactions
            .Where(t => t.Status == TransactionStatus.Captured)
            .Sum(t => t.Amount);
    }

    public decimal GetTotalRefunded()
    {
        return _transactions
            .Where(t => t.Status == TransactionStatus.Refunded)
            .Sum(t => t.Amount);
    }
}