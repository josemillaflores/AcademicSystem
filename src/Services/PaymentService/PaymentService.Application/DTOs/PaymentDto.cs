namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para transferir información del pago
/// </summary>
public record PaymentDto(
    Guid Id,
    string PaymentNumber,
    Guid StudentId,
    string StudentName,
    string StudentNumber,
    decimal Amount,
    string Currency,
    DateTime PaymentDate,
    string Method,
    string Status,
    string? GatewayResponse,
    List<TransactionDto> Transactions,
    DateTime CreatedAt
);

/// <summary>
/// DTO para creación de pago
/// </summary>
public record CreatePaymentDto(
    Guid StudentId,
    decimal Amount,
    string Method,
    string Currency = "USD"
);

/// <summary>
/// DTO para completar pago
/// </summary>
public record CompletePaymentDto(
    string TransactionId,
    string GatewayResponse
);