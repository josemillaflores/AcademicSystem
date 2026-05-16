public record AcademicLoad
{
    public int MaxHoursPerWeek { get; private set; }
    public int CurrentHours { get; private set; }
    public int RemainingHours => MaxHoursPerWeek - CurrentHours;
}