namespace AcademicSystem.EventBus;

public class PaymentCompletedEvent : IntegrationEvent
{
    public Guid PaymentId { get; }
    public Guid StudentId { get; }
    public decimal Amount { get; }
    public string TransactionId { get; }
    
    public PaymentCompletedEvent(Guid paymentId, Guid studentId, decimal amount, string transactionId)
    {
        PaymentId = paymentId;
        StudentId = studentId;
        Amount = amount;
        TransactionId = transactionId;
    }
}