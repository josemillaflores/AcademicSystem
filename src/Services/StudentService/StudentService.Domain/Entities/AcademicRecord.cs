namespace StudentService.Domain.Entities;

public class AcademicRecord
{
    public List<CourseEnrollment> Enrollments { get; private set; }
    public double GPA { get; private set; }
    public int TotalCredits { get; private set; }

    public AcademicRecord()
    {
        Enrollments = new List<CourseEnrollment>();
        GPA = 0;
        TotalCredits = 0;
    }

    public void AddEnrollment(Guid courseId)
    {
        Enrollments.Add(new CourseEnrollment(courseId, DateTime.UtcNow));
    }
}