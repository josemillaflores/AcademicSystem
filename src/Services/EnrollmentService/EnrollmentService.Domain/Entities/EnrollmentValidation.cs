using AcademicSystem.Common.Entities;
using EnrollmentService.Domain.Enums;

namespace EnrollmentService.Domain.Entities;

public class EnrollmentValidation : BaseEntity
{
    public ValidationType Type { get; private set; }
    public bool IsValid { get; private set; }
    public string Message { get; private set; }
    public DateTime ValidatedAt { get; private set; }
    public Guid EnrollmentId { get; private set; }

    private EnrollmentValidation() { }

    public EnrollmentValidation(ValidationType type, bool isValid, string message)
    {
        Type = type;
        IsValid = isValid;
        Message = message;
        ValidatedAt = DateTime.UtcNow;
    }
}