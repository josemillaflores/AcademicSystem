namespace TeacherService.Domain.ValueObjects;

public record TeacherName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }
    public string FullName => $"{FirstName} {(MiddleName != null ? MiddleName + " " : "")}{LastName}";

    public TeacherName(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
    }
}