public class Payment
{
    public Guid Id { get; private set; }
    public string PaymentNumber { get; private set; }
    public Guid StudentId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public List<Transaction> Transactions { get; private set; }
    
    public void Process()
    {
        Status = PaymentStatus.Processing;
        AddDomainEvent(new PaymentInitiatedEvent(Id, StudentId, Amount));
    }
    
    public void Complete(string transactionId)
    {
        Status = PaymentStatus.Completed;
        Transactions.Add(new Transaction(transactionId, Amount, DateTime.UtcNow));
        AddDomainEvent(new PaymentCompletedEvent(Id, StudentId, transactionId));
    }
    
    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        AddDomainEvent(new PaymentFailedEvent(Id, StudentId, reason));
    }
}
