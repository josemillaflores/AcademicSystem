namespace EnrollmentService.Application.DTOs;

/// <summary>
/// Resultado de validación de matrícula
/// </summary>
public class EnrollmentValidationResult
{
    public bool IsValid { get; set; }
    public bool StudentIsValid { get; set; }
    public bool CourseIsValid { get; set; }
    public bool PaymentsAreValid { get; set; }
    public bool PrerequisitesAreMet { get; set; }
    public StudentInfoDto? StudentInfo { get; set; }
    public CourseInfoDto? CourseInfo { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public EnrollmentValidationResult()
    {
        IsValid = false;
        StudentIsValid = false;
        CourseIsValid = false;
        PaymentsAreValid = false;
        PrerequisitesAreMet = false;
    }
}

/// <summary>
/// Solicitud de proceso de matrícula
/// </summary>
public record ProcessEnrollmentRequest(
    Guid StudentId,
    Guid CourseId,
    bool PaymentRequired = true,
    decimal Amount = 0,
    string PaymentMethod = "CreditCard"
);

/// <summary>
/// Resultado del proceso de matrícula
/// </summary>
public record ProcessEnrollmentResult(
    bool Success = false,
    Guid EnrollmentId = default,
    string Message = "",
    List<string> Errors = null!
)
{
    public ProcessEnrollmentResult() : this(false, Guid.Empty, "", new List<string>()) { }
}