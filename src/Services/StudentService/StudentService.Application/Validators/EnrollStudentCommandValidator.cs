using FluentValidation;

namespace StudentService.Application.Validators;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
        
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required");
        
        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course name is required")
            .MaximumLength(200).WithMessage("Course name cannot exceed 200 characters");
        
        RuleFor(x => x.Credits)
            .GreaterThan(0).WithMessage("Credits must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Credits cannot exceed 10 per course");
    }
}