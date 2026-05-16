using CourseService.Domain.ValueObjects;
using CourseService.Domain.Enums;

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
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string description, int credits, int maxCapacity)
    {
        Name = name;
        Description = description;
        Credits = credits;
        MaxCapacity = maxCapacity;
        UpdateTimestamp();
    }

    public Prerequisite AddPrerequisite(Guid requiredCourseId, string requiredCourseName, bool isMandatory = true)
    {
        if (_prerequisites.Any(p => p.RequiredCourseId == requiredCourseId))
            throw new InvalidOperationException("Prerequisite already exists");

        var prerequisite = new Prerequisite(requiredCourseId, requiredCourseName, isMandatory);
        _prerequisites.Add(prerequisite);
        UpdateTimestamp();
        
        return prerequisite;
    }

    public void RemovePrerequisite(Guid prerequisiteId)
    {
        var prerequisite = _prerequisites.FirstOrDefault(p => p.Id == prerequisiteId);
        if (prerequisite != null)
        {
            _prerequisites.Remove(prerequisite);
            UpdateTimestamp();
        }
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

    public void DecrementEnrollment()
    {
        if (CurrentEnrollment > 0)
        {
            CurrentEnrollment--;
            if (Status == CourseStatus.Full && CurrentEnrollment < MaxCapacity)
                Status = CourseStatus.Active;
            UpdateTimestamp();
        }
    }

    public bool HasAvailableSlots() => CurrentEnrollment < MaxCapacity;

    public void SetSchedule(Schedule schedule)
    {
        Schedule = schedule;
        UpdateTimestamp();
    }

    public void Activate()
    {
        Status = CourseStatus.Active;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        Status = CourseStatus.Cancelled;
        UpdateTimestamp();
    }
}