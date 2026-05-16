public class Transaction
{
    public Guid Id { get; private set; }
    public string TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? GatewayResponse { get; private set; }
}
