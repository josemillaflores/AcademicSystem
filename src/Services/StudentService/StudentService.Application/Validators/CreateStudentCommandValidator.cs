using FluentValidation;
using StudentService.Application.Commands;

namespace StudentService.Application.Validators;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches("^[a-zA-ZáéíóúñÑ\\s]+$").WithMessage("First name can only contain letters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches("^[a-zA-ZáéíóúñÑ\\s]+$").WithMessage("Last name can only contain letters");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .Matches("^[0-9\\-\\+\\s]+$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone can only contain numbers, spaces, hyphens and plus sign");
        
        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}