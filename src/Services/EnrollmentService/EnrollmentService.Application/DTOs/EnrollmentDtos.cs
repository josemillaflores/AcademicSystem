namespace EnrollmentService.Application.DTOs;

/// <summary>
/// DTO para transferir información de la matrícula
/// </summary>
public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentNumber,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    DateTime EnrollmentDate,
    string Period,
    string Status,
    string? RejectionReason,
    List<EnrollmentValidationDto> Validations,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO para validaciones de matrícula
/// </summary>
public record EnrollmentValidationDto(
    string Type,
    bool IsValid,
    string Message,
    DateTime ValidatedAt
);

/// <summary>
/// DTO para creación de matrícula
/// </summary>
public record CreateEnrollmentDto(
    Guid StudentId,
    Guid CourseId
);

/// <summary>
/// DTO para resultados paginados
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}