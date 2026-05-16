public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency = "USD")
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        Amount = amount;
        Currency = currency;
    }
}