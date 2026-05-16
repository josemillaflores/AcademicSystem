namespace TeacherService.Domain.ValueObjects;

public record TeacherId
{
    public string Value { get; }

    public TeacherId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Teacher number cannot be empty", nameof(value));

        Value = value.ToUpperInvariant().Trim();
    }
}