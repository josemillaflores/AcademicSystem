namespace StudentService.Domain.ValueObjects;

public record StudentId
{
    public string Value { get; }

    public StudentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Student number cannot be empty", nameof(value));
        
        if (value.Length > 20)
            throw new ArgumentException("Student number cannot exceed 20 characters", nameof(value));

        Value = value.ToUpperInvariant().Trim();
    }

    public static implicit operator string(StudentId studentId) => studentId.Value;
    public override string ToString() => Value;
}