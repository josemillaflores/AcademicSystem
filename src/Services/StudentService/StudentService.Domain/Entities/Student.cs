using AcademicSystem.Common.Entities;
using StudentService.Domain.ValueObjects;
using StudentService.Domain.Enums;

namespace StudentService.Domain.Entities;

public class Student : BaseEntity
{
    private readonly List<CourseEnrollment> _enrollments = new();

    public StudentName Name { get; private set; }
    public Email Email { get; private set; }
    public StudentId StudentNumber { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public AcademicRecord AcademicRecord { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    
    public IReadOnlyCollection<CourseEnrollment> Enrollments => _enrollments.AsReadOnly();

    private Student() { }

    public Student(StudentName name, Email email, StudentId studentNumber)
    {
        Name = name;
        Email = email;
        StudentNumber = studentNumber;
        EnrollmentDate = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
        AcademicRecord = new AcademicRecord();
        ContactInfo = new ContactInfo();
    }

    public void UpdatePersonalInfo(StudentName newName, Email newEmail)
    {
        Name = newName;
        Email = newEmail;
        UpdateTimestamp();
    }

    public void UpdateContactInfo(string? phone, string? address, string? city = null, string? country = null)
    {
        ContactInfo = new ContactInfo(phone, address, city, country);
        UpdateTimestamp();
    }

    public void EnrollInCourse(Guid courseId, string courseName, int credits)
    {
        if (Status != EnrollmentStatus.Active)
            throw new InvalidOperationException($"Student is not active. Current status: {Status}");

        if (_enrollments.Any(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Approved))
            throw new InvalidOperationException("Student is already enrolled in this course");

        var enrollment = new CourseEnrollment(courseId, courseName, credits, DateTime.UtcNow);
        _enrollments.Add(enrollment);
        
        AcademicRecord.AddEnrollment(courseId, credits);
        UpdateTimestamp();
    }
}