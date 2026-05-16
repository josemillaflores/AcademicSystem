namespace TeacherService.Application.DTOs;

/// <summary>
/// DTO para cursos asignados a un docente
/// </summary>
public record TeacherCourseDto(
    Guid AssignmentId,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    int Credits,
    int HoursPerWeek,
    DateTime AssignmentDate,
    string? Period,
    bool IsActive
);