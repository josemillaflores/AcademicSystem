// CourseService.Domain/Entities/Course.cs
public class Course
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Credits { get; private set; }
    public int TotalHours { get; private set; }
    public int MaxCapacity { get; private set; }
    public int CurrentEnrollment { get; private set; }
    public CourseStatus Status { get; private set; }
    public List<Prerequisite> Prerequisites { get; private set; }
    public Schedule Schedule { get; private set; }
    
    public bool HasAvailableSlots() => CurrentEnrollment < MaxCapacity;
    public bool CanEnroll(Student student) => ValidatePrerequisites(student);
}

 