using EnrollmentService.Application.Services;

namespace EnrollmentService.Application.DTOs;

/// <summary>
/// DTO con información completa de matrícula (API Composition)
/// </summary>
public class CompleteEnrollmentDto
{
    public EnrollmentDto? Enrollment { get; set; }
    public StudentInfoDto? Student { get; set; }
    public CourseInfoDto? Course { get; set; }
    public List<PaymentInfoDto>? Payments { get; set; }
    public DateTime ComposedAt { get; set; }
}

/// <summary>
/// Información del estudiante para composición
/// </summary>
public record StudentInfoDto(
    Guid Id,
    string FullName,
    string Email,
    string StudentNumber,
    string Status,
    List<CompletedCourseDto> CompletedCourses
);

public record CompletedCourseDto(Guid CourseId, string CourseName, double Grade);

/// <summary>
/// Información del curso para composición
/// </summary>
public record CourseInfoDto(
    Guid Id,
    string Code,
    string Name,
    int Credits,
    int MaxCapacity,
    int CurrentEnrollment,
    string Status,
    bool HasAvailableSlots,
    List<PrerequisiteInfoDto> Prerequisites
);

public record PrerequisiteInfoDto(
    Guid RequiredCourseId,
    string RequiredCourseName,
    bool IsMandatory
);

/// <summary>
/// Información de pagos para composición
/// </summary>
public record PaymentInfoDto(
    Guid Id,
    decimal Amount,
    string Status,
    DateTime PaymentDate
);