using AcademicSystem.Common.Entities;
using EnrollmentService.Domain.Enums;
using EnrollmentService.Domain.ValueObjects;

namespace EnrollmentService.Domain.Entities;

public class Enrollment : BaseEntity
{
    private readonly List<EnrollmentValidation> _validations = new();

    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public string StudentName { get; private set; }
    public string StudentNumber { get; private set; }
    public string CourseName { get; private set; }
    public string CourseCode { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentPeriod Period { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    
    public IReadOnlyCollection<EnrollmentValidation> Validations => _validations.AsReadOnly();

    private Enrollment() { }

    public Enrollment(Guid studentId, Guid courseId, string periodName)
    {
        StudentId = studentId;
        CourseId = courseId;
        EnrollmentDate = DateTime.UtcNow;
        Period = new EnrollmentPeriod(periodName);
        Status = EnrollmentStatus.Pending;
    }

    public void Approve()
    {
        if (Status != EnrollmentStatus.Pending)
            throw new InvalidOperationException($"Cannot approve enrollment with status {Status}");

        Status = EnrollmentStatus.Approved;
        UpdateTimestamp();
    }

    public void Reject(string reason)
    {
        if (Status != EnrollmentStatus.Pending)
            throw new InvalidOperationException($"Cannot reject enrollment with status {Status}");

        Status = EnrollmentStatus.Rejected;
        RejectionReason = reason;
        UpdateTimestamp();
    }
}