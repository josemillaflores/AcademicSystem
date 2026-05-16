namespace CourseService.Application.DTOs;

/// <summary>
/// DTO para estadísticas de cursos
/// </summary>
public record CourseStatisticsDto(
    int TotalCourses,
    int ActiveCourses,
    int FullCourses,
    int CancelledCourses,
    double AverageCredits,
    int TotalEnrollments,
    double AverageEnrollmentRate,
    double TotalRevenue,
    Dictionary<string, int> CoursesByDepartment,
    Dictionary<string, int> CoursesByStatus
);