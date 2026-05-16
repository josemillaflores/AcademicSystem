using StudentService.Domain.Enums;

namespace StudentService.Domain.Entities;

/// <summary>
/// Entidad interna del agregado Student
/// Representa la inscripción a un curso
/// </summary>
public class CourseEnrollment : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string CourseName { get; private set; }
    public int Credits { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public double? Grade { get; private set; }
    public Guid StudentId { get; private set; }

    private CourseEnrollment() { }

    public CourseEnrollment(Guid courseId, string courseName, int credits, DateTime enrollmentDate)
    {
        CourseId = courseId;
        CourseName = courseName;
        Credits = credits;
        EnrollmentDate = enrollmentDate;
        Status = EnrollmentStatus.Pending;
    }

    public void Approve()
    {
        Status = EnrollmentStatus.Approved;
        UpdateTimestamp();
    }

    public void Reject()
    {
        Status = EnrollmentStatus.Rejected;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        Status = EnrollmentStatus.Cancelled;
        UpdateTimestamp();
    }

    public void Complete(double grade)
    {
        if (Status != EnrollmentStatus.Approved)
            throw new InvalidOperationException("Cannot complete enrollment that is not approved");
        
        Grade = grade;
        Status = EnrollmentStatus.Completed;
        UpdateTimestamp();
    }
}