using TeacherService.Domain.ValueObjects;
using TeacherService.Domain.Enums;
using AcademicSystem.Common;

namespace TeacherService.Domain.Entities;

public class Teacher : BaseEntity
{
    private readonly List<Specialty> _specialties = new();
    private readonly List<CourseAssignment> _courseAssignments = new();

    public TeacherName Name { get; private set; }
    public Email Email { get; private set; }
    public TeacherId TeacherNumber { get; private set; }
    public DateTime HireDate { get; private set; }
    public TeacherStatus Status { get; private set; }
    public AcademicLoad AcademicLoad { get; private set; }
    
    public IReadOnlyCollection<Specialty> Specialties => _specialties.AsReadOnly();
    public IReadOnlyCollection<CourseAssignment> CourseAssignments => _courseAssignments.AsReadOnly();

    private Teacher() { }

    public Teacher(TeacherName name, Email email, TeacherId teacherNumber, DateTime hireDate)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        TeacherNumber = teacherNumber;
        HireDate = hireDate;
        Status = TeacherStatus.Active;
        AcademicLoad = new AcademicLoad(40, 0);
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(TeacherName newName, Email newEmail)
    {
        Name = newName;
        Email = newEmail;
        UpdateTimestamp();
    }

    public void AddSpecialty(Specialty specialty)
    {
        if (!_specialties.Any(s => s.Name == specialty.Name))
        {
            _specialties.Add(specialty);
            UpdateTimestamp();
        }
    }

    public void RemoveSpecialty(Guid specialtyId)
    {
        var specialty = _specialties.FirstOrDefault(s => s.Id == specialtyId);
        if (specialty != null)
        {
            _specialties.Remove(specialty);
            UpdateTimestamp();
        }
    }

    public CourseAssignment AssignCourse(Guid courseId, string courseName, int hoursPerWeek, string? period = null)
    {
        if (AcademicLoad.CurrentHours + hoursPerWeek > AcademicLoad.MaxHoursPerWeek)
            throw new InvalidOperationException($"Cannot assign course. Max hours per week exceeded. Current: {AcademicLoad.CurrentHours}, Max: {AcademicLoad.MaxHoursPerWeek}");

        var assignment = new CourseAssignment(courseId, courseName, hoursPerWeek, period ?? GetCurrentPeriod());
        _courseAssignments.Add(assignment);
        AcademicLoad.AddHours(hoursPerWeek);
        UpdateTimestamp();
        
        return assignment;
    }

    public void RemoveCourseAssignment(Guid assignmentId)
    {
        var assignment = _courseAssignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment != null)
        {
            _courseAssignments.Remove(assignment);
            AcademicLoad.RemoveHours(assignment.HoursPerWeek);
            UpdateTimestamp();
        }
    }

    public void SetOnLeave(string reason)
    {
        Status = TeacherStatus.OnLeave;
        UpdateTimestamp();
    }

    public void Retire()
    {
        Status = TeacherStatus.Retired;
        UpdateTimestamp();
    }

    public void Activate()
    {
        Status = TeacherStatus.Active;
        UpdateTimestamp();
    }

    private string GetCurrentPeriod()
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var semester = now.Month <= 7 ? "1" : "2";
        return $"{year}-{semester}";
    }
}