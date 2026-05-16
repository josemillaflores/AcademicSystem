namespace CourseService.Application.DTOs;

/// <summary>
/// DTO para prerrequisito de curso
/// </summary>
public record PrerequisiteDto(
    Guid Id,
    Guid RequiredCourseId,
    string RequiredCourseName,
    string RequiredCourseCode,
    bool IsMandatory
);

/// <summary>
/// DTO para creación de prerrequisito
/// </summary>
public record CreatePrerequisiteDto(
    Guid RequiredCourseId,
    bool IsMandatory
);