public class EnrollmentValidation
{
    public Guid Id { get; private set; }
    public ValidationType Type { get; private set; }
    public bool IsValid { get; private set; }
    public string Message { get; private set; }
    public DateTime ValidatedAt { get; private set; }
}