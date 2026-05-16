namespace StudentService.Application.DTOs;

/// <summary>
/// DTO para cursos en los que está inscrito un estudiante
/// </summary>
public record StudentCourseDto(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    int Credits,
    DateTime EnrollmentDate,
    string Status,
    double? Grade,
    string? GradeLetter
);