namespace TeacherService.Application.DTOs;

/// <summary>
/// DTO para carga académica del docente
/// </summary>
public record AcademicLoadDto(
    int MaxHoursPerWeek,
    int CurrentHours,
    int RemainingHours,
    double UtilizationPercentage,
    List<AssignedCourseLoadDto> AssignedCourses
);

public record AssignedCourseLoadDto(
    Guid CourseId,
    string CourseName,
    int HoursPerWeek,
    int StudentsCount
);