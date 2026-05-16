namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para balance de estudiante
/// </summary>
public record StudentBalanceDto(
    Guid StudentId,
    string StudentName,
    string StudentNumber,
    decimal TotalPaid,
    decimal TotalPending,
    decimal Balance,
    string BalanceStatus,
    List<PaymentSummaryDto> RecentPayments
);

public record PaymentSummaryDto(
    Guid PaymentId,
    string PaymentNumber,
    decimal Amount,
    DateTime PaymentDate,
    string Status
);