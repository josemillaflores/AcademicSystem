namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para transacción de pago
/// </summary>
public record TransactionDto(
    Guid Id,
    string TransactionId,
    decimal Amount,
    DateTime TransactionDate,
    string Status,
    string? GatewayResponse
)
{
    public TransactionDto() : this(Guid.Empty, string.Empty, 0, DateTime.MinValue, string.Empty, null) { }
}