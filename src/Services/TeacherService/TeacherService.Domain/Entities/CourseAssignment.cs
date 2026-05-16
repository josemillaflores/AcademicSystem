using AcademicSystem.Common;

namespace TeacherService.Domain.Entities;

public class CourseAssignment : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string CourseName { get; private set; }
    public int Credits { get; private set; }
    public int HoursPerWeek { get; private set; }
    public int StudentsCount { get; private set; }
    public DateTime AssignmentDate { get; private set; }
    public string Period { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TeacherId { get; private set; }

    private CourseAssignment() { }

    public CourseAssignment(Guid courseId, string courseName, int hoursPerWeek, string period)
    {
        CourseId = courseId;
        CourseName = courseName;
        HoursPerWeek = hoursPerWeek;
        Period = period;
        AssignmentDate = DateTime.UtcNow;
        IsActive = true;
        StudentsCount = 0;
        Credits = 0;
    }

    public void UpdateStudentCount(int count)
    {
        StudentsCount = count;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }
}