namespace PaymentService.Domain.Enums;

public enum TransactionStatus
{
    Initiated,
    Authorized,
    Captured,
    Failed,
    Refunded
}