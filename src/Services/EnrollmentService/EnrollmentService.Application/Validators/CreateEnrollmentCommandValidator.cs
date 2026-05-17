using EnrollmentService.Application.Commands;
using FluentValidation;

namespace EnrollmentService.Application.Validators;

public class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
{
    public CreateEnrollmentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
        
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required");
        
        RuleFor(x => x.Period)
            .MaximumLength(20).WithMessage("Period cannot exceed 20 characters")
            .Matches(@"^\d{4}-\d$").When(x => !string.IsNullOrEmpty(x.Period))
            .WithMessage("Period must be in format YYYY-S (e.g., 2024-1)");
    }
}