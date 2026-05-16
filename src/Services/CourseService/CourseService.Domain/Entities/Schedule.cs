namespace CourseService.Domain.ValueObjects;

public record Schedule
{
    public DayOfWeek Day { get; }
    public TimeSpan StartTime { get; }
    public TimeSpan EndTime { get; }
    public string Classroom { get; }

    public Schedule(DayOfWeek day, TimeSpan startTime, TimeSpan endTime, string classroom)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");
        
        if (string.IsNullOrWhiteSpace(classroom))
            throw new ArgumentException("Classroom cannot be empty");

        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        Classroom = classroom;
    }

    public int DurationHours => (int)(EndTime - StartTime).TotalHours;
}