using FluentValidation;
using TeacherService.Application.Commands;

namespace TeacherService.Application.Validators;

public class AssignCourseToTeacherCommandValidator : AbstractValidator<AssignCourseToTeacherCommand>
{
    public AssignCourseToTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher ID is required");
        
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course ID is required");
        
        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course name is required")
            .MaximumLength(200);
        
        RuleFor(x => x.HoursPerWeek)
            .GreaterThan(0).WithMessage("Hours per week must be greater than 0")
            .LessThanOrEqualTo(40).WithMessage("Hours per week cannot exceed 40");
        
        RuleFor(x => x.Period)
            .MaximumLength(20).WithMessage("Period cannot exceed 20 characters");
    }
}