namespace StudentService.Application.DTOs;

/// <summary>
/// DTO para el historial académico del estudiante
/// </summary>
public class AcademicRecordDto
{
    /// <summary>
    /// Promedio general (GPA)
    /// </summary>
    public double GPA { get; set; }
    
    /// <summary>
    /// Total de créditos aprobados
    /// </summary>
    public int TotalCredits { get; set; }
    
    /// <summary>
    /// Créditos requeridos para graduación
    /// </summary>
    public int RequiredCreditsForGraduation { get; set; } = 180;
    
    /// <summary>
    /// Créditos restantes para graduación
    /// </summary>
    public int RemainingCredits => RequiredCreditsForGraduation - TotalCredits;
    
    /// <summary>
    /// Porcentaje de avance en la carrera
    /// </summary>
    public double CompletionPercentage => (double)TotalCredits / RequiredCreditsForGraduation * 100;
    
    /// <summary>
    /// Cursos completados
    /// </summary>
    public List<AcademicCompletedCourseDto> CompletedCourses { get; set; } = new();
    
    /// <summary>
    /// Semestre actual
    /// </summary>
    public int CurrentSemester { get; set; }
    
    /// <summary>
    /// Estado académico (Good Standing, Probation, etc)
    /// </summary>
    public string AcademicStatus { get; set; } = "Good Standing";
}

/// <summary>
/// DTO para curso completado
/// </summary>
public record AcademicCompletedCourseDto(
    Guid CourseId,
    string CourseName,
    string CourseCode,
    int Credits,
    double Grade,
    string GradeLetter,
    DateTime CompletionDate,
    int Semester
)
{
    public string GradeLetter => Grade switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
}