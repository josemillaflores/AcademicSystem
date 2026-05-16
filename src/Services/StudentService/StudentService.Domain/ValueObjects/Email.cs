namespace StudentService.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");
        if (!value.Contains("@"))
            throw new ArgumentException("Invalid email format");
            
        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
}