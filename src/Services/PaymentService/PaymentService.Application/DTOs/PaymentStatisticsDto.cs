namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para estadísticas de pagos
/// </summary>
public record PaymentStatisticsDto(
    int TotalPayments,
    int CompletedPayments,
    int PendingPayments,
    int FailedPayments,
    decimal TotalRevenue,
    decimal AveragePaymentAmount,
    decimal RevenueThisMonth,
    double SuccessRate,
    Dictionary<string, decimal> RevenueByMethod,
    Dictionary<string, int> PaymentsByStatus
);