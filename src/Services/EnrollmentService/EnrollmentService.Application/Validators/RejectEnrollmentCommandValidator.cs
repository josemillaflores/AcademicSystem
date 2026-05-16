using FluentValidation;

namespace EnrollmentService.Application.Validators;

public class RejectEnrollmentCommandValidator : AbstractValidator<RejectEnrollmentCommand>
{
    public RejectEnrollmentCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required");
        
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}