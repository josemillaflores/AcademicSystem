using AcademicSystem.Common.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class Transaction : BaseEntity
{
    public string TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? GatewayResponse { get; private set; }
    public Guid PaymentId { get; private set; }

    private Transaction() { }

    public Transaction(string transactionId, decimal amount, string? gatewayResponse = null)
    {
        TransactionId = transactionId;
        Amount = amount;
        TransactionDate = DateTime.UtcNow;
        Status = TransactionStatus.Initiated;
        GatewayResponse = gatewayResponse;
    }
}