namespace StudentService.Domain.Entities;

/// <summary>
/// Entidad interna del agregado Student
/// Representa un curso completado
/// </summary>
public class CompletedCourse
{
    public Guid CourseId { get; private set; }
    public string CourseName { get; private set; }
    public int Credits { get; private set; }
    public double Grade { get; private set; }
    public DateTime CompletionDate { get; private set; }
    public int Semester { get; private set; }

    public CompletedCourse(Guid courseId, double grade)
    {
        CourseId = courseId;
        Grade = grade;
        CompletionDate = DateTime.UtcNow;
    }

    public void UpdateGrade(double grade)
    {
        Grade = grade;
    }
}