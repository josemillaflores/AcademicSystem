using StudentService.Domain.Events;
using StudentService.Domain.ValueObjects;
using StudentService.Domain.Enums;

namespace StudentService.Domain.Entities;

/// <summary>
/// Agregado principal: Student
/// Representa la raíz del agregado de estudiantes
/// </summary>
public class Student : BaseEntity
{
    // Campos privados
    private readonly List<CourseEnrollment> _enrollments = new();
    private readonly List<DomainEvent> _domainEvents = new();

    // Propiedades públicas
    public StudentName Name { get; private set; }
    public Email Email { get; private set; }
    public StudentId StudentNumber { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public AcademicRecord AcademicRecord { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    
    // Propiedades de navegación
    public IReadOnlyCollection<CourseEnrollment> Enrollments => _enrollments.AsReadOnly();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructor para EF Core
    private Student() { }

    // Constructor de negocio
    public Student(StudentName name, Email email, StudentId studentNumber)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        StudentNumber = studentNumber ?? throw new ArgumentNullException(nameof(studentNumber));
        EnrollmentDate = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
        AcademicRecord = new AcademicRecord();
        ContactInfo = new ContactInfo();
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new StudentCreatedEvent(Id, name.ToString(), email.Value, studentNumber.Value));
    }

    // Métodos de negocio
    public void UpdatePersonalInfo(StudentName newName, Email newEmail)
    {
        var oldName = Name;
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        UpdateTimestamp();

        AddDomainEvent(new StudentInfoUpdatedEvent(Id, oldName.ToString(), newName.ToString()));
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

        AddDomainEvent(new StudentEnrolledEvent(Id, courseId, courseName));
    }

    public void CompleteCourse(Guid courseId, double grade)
    {
        var enrollment = _enrollments.FirstOrDefault(e => e.CourseId == courseId);
        if (enrollment == null)
            throw new InvalidOperationException($"Student is not enrolled in course {courseId}");

        enrollment.Complete(grade);
        AcademicRecord.AddGrade(courseId, grade);
        UpdateTimestamp();

        AddDomainEvent(new CourseCompletedEvent(Id, courseId, grade));
    }

    public void Graduate()
    {
        if (AcademicRecord.TotalCredits >= AcademicRecord.RequiredCreditsForGraduation)
        {
            Status = EnrollmentStatus.Graduated;
            AddDomainEvent(new StudentGraduatedEvent(Id, Name.ToString()));
        }
        else
        {
            throw new InvalidOperationException(
                $"Cannot graduate. Required credits: {AcademicRecord.RequiredCreditsForGraduation}, " +
                $"Current credits: {AcademicRecord.TotalCredits}");
        }
    }

    public void Suspend(string reason)
    {
        Status = EnrollmentStatus.Suspended;
        AddDomainEvent(new StudentSuspendedEvent(Id, Name.ToString(), reason));
    }

    public void Reactivate()
    {
        Status = EnrollmentStatus.Active;
        AddDomainEvent(new StudentReactivatedEvent(Id, Name.ToString()));
    }

    private void AddDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}