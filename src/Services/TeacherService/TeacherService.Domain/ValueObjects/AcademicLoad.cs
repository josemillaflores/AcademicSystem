namespace TeacherService.Domain.ValueObjects;

public record AcademicLoad
{
    public int MaxHoursPerWeek { get; private set; }
    public int CurrentHours { get; private set; }
    public int RemainingHours => MaxHoursPerWeek - CurrentHours;

    public AcademicLoad(int maxHoursPerWeek, int currentHours)
    {
        MaxHoursPerWeek = maxHoursPerWeek;
        CurrentHours = currentHours;
    }

    public void AddHours(int hours)
    {
        if (CurrentHours + hours > MaxHoursPerWeek)
            throw new InvalidOperationException("Cannot exceed maximum hours per week");
        
        CurrentHours += hours;
    }

    public void RemoveHours(int hours)
    {
        if (CurrentHours - hours < 0)
            throw new InvalidOperationException("Cannot have negative hours");
        
        CurrentHours -= hours;
    }
}