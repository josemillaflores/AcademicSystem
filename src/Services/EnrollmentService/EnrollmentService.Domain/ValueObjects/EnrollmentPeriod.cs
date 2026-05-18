namespace EnrollmentService.Domain.ValueObjects;

public record EnrollmentPeriod
{
    public string Name { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    public EnrollmentPeriod(string name)
    {
        Name = name;
        
        // Parse period name like "2024-1" or "2024-2"
        var parts = name.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int semester))
        {
            if (semester == 1)
            {
                StartDate = new DateTime(year, 1, 15, 0, 0, 0, DateTimeKind.Utc);
                EndDate = new DateTime(year, 6, 30, 23, 59, 59, DateTimeKind.Utc);
            }
            else
            {
                StartDate = new DateTime(year, 7, 15, 0, 0, 0, DateTimeKind.Utc);
                EndDate = new DateTime(year, 12, 20, 23, 59, 59, DateTimeKind.Utc);
            }
        }
        else
        {
            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow.AddMonths(6);
        }
    }
}