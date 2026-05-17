using CourseService.Application.Commands;
using FluentValidation;

namespace CourseService.Application.Validators;

public class AddPrerequisiteCommandValidator : AbstractValidator<AddPrerequisiteCommand>
{
    public AddPrerequisiteCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required");
        
        RuleFor(x => x.RequiredCourseId)
            .NotEmpty().WithMessage("Required course ID is required")
            .NotEqual(x => x.CourseId).WithMessage("A course cannot be a prerequisite of itself");
    }
}