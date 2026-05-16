using EnrollmentService.Domain.ValueObjects;
using EnrollmentService.Domain.Enums;

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
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCourseInfo(string name, string code)
    {
        CourseName = name;
        CourseCode = code;
        UpdateTimestamp();
    }

    public void UpdateStudentInfo(string name, string number)
    {
        StudentName = name;
        StudentNumber = number;
        UpdateTimestamp();
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

    public void Cancel()
    {
        if (Status == EnrollmentStatus.Approved || Status == EnrollmentStatus.Pending)
        {
            Status = EnrollmentStatus.Cancelled;
            UpdateTimestamp();
        }
        else
        {
            throw new InvalidOperationException($"Cannot cancel enrollment with status {Status}");
        }
    }

    public void AddValidation(ValidationType type, bool isValid, string message)
    {
        var validation = new EnrollmentValidation(type, isValid, message);
        _validations.Add(validation);
        UpdateTimestamp();
    }

    public bool IsValid()
    {
        return Status == EnrollmentStatus.Pending && 
               _validations.All(v => v.IsValid);
    }
}