namespace StudentService.Application.DTOs;

/// <summary>
/// DTO para estadísticas de estudiantes
/// </summary>
public record StudentStatisticsDto(
    int TotalStudents,
    int ActiveStudents,
    int InactiveStudents,
    int GraduatedStudents,
    int SuspendedStudents,
    double AverageAge,
    int NewStudentsThisMonth,
    double AverageGPA,
    Dictionary<string, int> StudentsByProgram,
    Dictionary<string, int> StudentsByStatus
);