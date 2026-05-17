using FluentValidation;
using PaymentService.Application.Commands;

namespace PaymentService.Application.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Amount cannot exceed 100,000");
        
        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(m => new[] { "CreditCard", "DebitCard", "BankTransfer", "Cash", "Scholarship" }.Contains(m))
            .WithMessage("Invalid payment method");
        
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., USD, EUR)")
            .Matches("^[A-Z]+$").WithMessage("Currency must be uppercase letters");
    }
}