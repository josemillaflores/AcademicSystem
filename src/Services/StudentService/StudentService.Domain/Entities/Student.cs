using StudentService.Domain.ValueObjects;
using StudentService.Domain.Events;

namespace StudentService.Domain.Entities;

public class Student
{
    public Guid Id { get; private set; }
    public StudentName Name { get; private set; }
    public Email Email { get; private set; }
    public StudentId StudentNumber { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public AcademicRecord AcademicRecord { get; private set; }
    private readonly List<DomainEvent> _events = new();
    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    private Student() { }

    public Student(StudentName name, Email email, StudentId studentNumber)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        StudentNumber = studentNumber;
        EnrollmentDate = DateTime.UtcNow;
        Status = EnrollmentStatus.Active;
        AcademicRecord = new AcademicRecord();
    }

    public void UpdatePersonalInfo(StudentName name, Email email)
    {
        Name = name;
        Email = email;
        AddEvent(new StudentInfoUpdatedEvent(Id, name, email));
    }

    public void EnrollInCourse(Guid courseId)
    {
        AcademicRecord.AddEnrollment(courseId);
        AddEvent(new StudentEnrolledEvent(Id, courseId));
    }

    private void AddEvent(DomainEvent @event) => _events.Add(@event);
    public void ClearEvents() => _events.Clear();
}

public enum EnrollmentStatus
{
    Active,
    Inactive,
    Graduated,
    Suspended
}