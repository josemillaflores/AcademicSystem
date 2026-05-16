namespace TeacherService.Application.DTOs;

/// <summary>
/// DTO para estadísticas de docentes
/// </summary>
public record TeacherStatisticsDto(
    int TotalTeachers,
    int ActiveTeachers,
    int OnLeaveTeachers,
    int RetiredTeachers,
    double AverageYearsOfService,
    int NewTeachersThisYear,
    Dictionary<string, int> TeachersBySpecialty,
    Dictionary<string, int> TeachersByStatus
);