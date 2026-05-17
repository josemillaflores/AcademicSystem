namespace StudentService.Domain.Entities;

/// <summary>
/// Entidad interna del agregado Student
/// Representa el historial académico
/// </summary>
public class AcademicRecord
{
    private readonly List<CompletedCourse> _completedCourses = new();
    
    public double GPA { get; private set; }
    public int TotalCredits { get; private set; }
    public int RequiredCreditsForGraduation { get; private set; } = 180;
    public IReadOnlyCollection<CompletedCourse> CompletedCourses => _completedCourses.AsReadOnly();

    public AcademicRecord()
    {
        GPA = 0;
        TotalCredits = 0;
    }

    public void AddEnrollment(Guid courseId, int credits)
    {
        TotalCredits += credits;
        CalculateGPA();
    }

    public void AddGrade(Guid courseId, double grade)
    {
        var course = _completedCourses.FirstOrDefault(c => c.CourseId == courseId);
        if (course != null)
        {
            course.UpdateGrade(grade);
        }
        else
        {
            _completedCourses.Add(new CompletedCourse(courseId, grade));
        }
        CalculateGPA();
    }

    private void CalculateGPA()
    {
        if (_completedCourses.Count == 0)
        {
            GPA = 0;
            return;
        }

        GPA = Math.Round(_completedCourses.Average(c => c.Grade), 2);
    }
}