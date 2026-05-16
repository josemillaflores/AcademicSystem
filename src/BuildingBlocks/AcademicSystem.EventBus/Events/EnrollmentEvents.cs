namespace AcademicSystem.EventBus;

public class EnrollmentApprovedEvent : IntegrationEvent
{
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    
    public EnrollmentApprovedEvent(Guid enrollmentId, Guid studentId, Guid courseId)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
    }
}

public class EnrollmentRejectedEvent : IntegrationEvent
{
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public string Reason { get; }

    public EnrollmentRejectedEvent(Guid enrollmentId, Guid studentId, Guid courseId, string reason)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        Reason = reason;
    }
}

public class EnrollmentCompletedEvent : IntegrationEvent
{
    public Guid EnrollmentId { get; }
    public Guid StudentId { get; }
    public Guid CourseId { get; }

    public EnrollmentCompletedEvent(Guid enrollmentId, Guid studentId, Guid courseId)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
    }
}