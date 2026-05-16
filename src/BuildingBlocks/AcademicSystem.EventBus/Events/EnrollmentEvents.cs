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