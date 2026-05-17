using AcademicSystem.Common.Entities;
using CourseService.Domain.Enums;
using CourseService.Domain.ValueObjects;

namespace CourseService.Domain.Entities;

public class Course : BaseEntity
{
    private readonly List<Prerequisite> _prerequisites = new();

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Credits { get; private set; }
    public int TotalHours { get; private set; }
    public int MaxCapacity { get; private set; }
    public int CurrentEnrollment { get; private set; }
    public CourseStatus Status { get; private set; }
    public Schedule? Schedule { get; private set; }
    
    public IReadOnlyCollection<Prerequisite> Prerequisites => _prerequisites.AsReadOnly();

    private Course() { }

    public Course(string code, string name, string description, int credits, int totalHours, int maxCapacity)
    {
        Code = code;
        Name = name;
        Description = description;
        Credits = credits;
        TotalHours = totalHours;
        MaxCapacity = maxCapacity;
        CurrentEnrollment = 0;
        Status = CourseStatus.Active;
    }

    public void UpdateInfo(string name, string description, int credits, int maxCapacity)
    {
        Name = name;
        Description = description;
        Credits = credits;
        MaxCapacity = maxCapacity;
        UpdateTimestamp();
    }

    public bool IncrementEnrollment()
    {
        if (CurrentEnrollment >= MaxCapacity)
            return false;

        CurrentEnrollment++;
        
        if (CurrentEnrollment == MaxCapacity)
            Status = CourseStatus.Full;
            
        UpdateTimestamp();
        return true;
    }

    public bool HasAvailableSlots() => CurrentEnrollment < MaxCapacity;
}