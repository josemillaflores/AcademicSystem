namespace AcademicSystem.Contracts.Payments;

public record PaymentResponse(
    Guid Id,
    string PaymentNumber,
    Guid StudentId,
    decimal Amount,
    DateTime PaymentDate,
    string Method,
    string Status
);

public record CreatePaymentRequest(
    Guid StudentId,
    decimal Amount,
    string Method
);