public record EnrollmentPeriod
{
    public string Name { get; }  // "Regular", "Late", "Special"
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}