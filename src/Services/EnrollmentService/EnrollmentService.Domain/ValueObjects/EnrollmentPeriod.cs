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
                StartDate = new DateTime(year, 1, 15);
                EndDate = new DateTime(year, 6, 30);
            }
            else
            {
                StartDate = new DateTime(year, 7, 15);
                EndDate = new DateTime(year, 12, 20);
            }
        }
        else
        {
            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow.AddMonths(6);
        }
    }
}