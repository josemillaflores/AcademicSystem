namespace StudentService.Domain.ValueObjects;

/// <summary>
/// Value Object: Nombre del estudiante
/// Inmutable, sin identidad propia
/// </summary>
public record StudentName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }
    public string FullName => $"{FirstName} {(MiddleName != null ? MiddleName + " " : "")}{LastName}";

    public StudentName(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        
        if (firstName.Length > 50)
            throw new ArgumentException("First name cannot exceed 50 characters", nameof(firstName));
        
        if (lastName.Length > 50)
            throw new ArgumentException("Last name cannot exceed 50 characters", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
    }

    public override string ToString() => FullName;
}