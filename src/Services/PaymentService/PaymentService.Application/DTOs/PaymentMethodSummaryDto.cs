namespace PaymentService.Application.DTOs;

/// <summary>
/// DTO para resumen de pagos por método
/// </summary>
public record PaymentMethodSummaryDto(
    string PaymentMethod,
    int TotalPayments,
    decimal TotalAmount,
    decimal AverageAmount,
    double PercentageOfTotal
);