namespace StudentService.Application.DTOs;

/// <summary>
/// DTO para transferir información del estudiante
/// </summary>
public record StudentDto(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string StudentNumber,
    DateTime EnrollmentDate,
    string Status,
    int TotalCredits,
    double GPA,
    string? Phone,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public StudentDto() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, string.Empty, 0, 0.0, null, DateTime.MinValue, null) { }
}

/// <summary>
/// DTO para creación de estudiante
/// </summary>
public record CreateStudentDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null
);

/// <summary>
/// DTO para actualización de estudiante
/// </summary>
public record UpdateStudentDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null
);

/// <summary>
/// DTO para curso completado por estudiante
/// </summary>
public record CompletedCourseDto(
    Guid CourseId,
    string CourseName,
    int Credits,
    double Grade,
    DateTime CompletionDate
)
{
    public CompletedCourseDto() : this(Guid.Empty, string.Empty, 0, 0.0, DateTime.MinValue) { }
}