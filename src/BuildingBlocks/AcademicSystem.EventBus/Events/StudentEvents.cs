public class StudentCreatedEvent : IntegrationEvent
{
    public Guid StudentId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string StudentNumber { get; }
    
    public StudentCreatedEvent(Guid studentId, string firstName, string lastName, string email, string studentNumber)
    {
        StudentId = studentId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        StudentNumber = studentNumber;
    }
}
