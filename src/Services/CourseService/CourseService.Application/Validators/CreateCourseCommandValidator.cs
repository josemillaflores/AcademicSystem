using FluentValidation;

namespace CourseService.Application.Validators;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Course code is required")
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters")
            .Matches("^[A-Z0-9]+$").WithMessage("Course code can only contain uppercase letters and numbers");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required")
            .MaximumLength(200).WithMessage("Course name cannot exceed 200 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
        
        RuleFor(x => x.Credits)
            .GreaterThan(0).WithMessage("Credits must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Credits cannot exceed 10");
        
        RuleFor(x => x.TotalHours)
            .GreaterThan(0).WithMessage("Total hours must be greater than 0")
            .LessThanOrEqualTo(200).WithMessage("Total hours cannot exceed 200");
        
        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Max capacity must be greater than 0")
            .LessThanOrEqualTo(500).WithMessage("Max capacity cannot exceed 500");
    }
}