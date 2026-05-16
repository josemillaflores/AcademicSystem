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
);