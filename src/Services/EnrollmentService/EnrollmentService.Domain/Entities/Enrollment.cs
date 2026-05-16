public class Enrollment
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public EnrollmentPeriod Period { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public List<EnrollmentValidation> Validations { get; private set; }
    
    public void Approve()
    {
        Status = EnrollmentStatus.Approved;
        AddDomainEvent(new EnrollmentApprovedEvent(Id, StudentId, CourseId));
    }
    
    public void Reject(string reason)
    {
        Status = EnrollmentStatus.Rejected;
        RejectionReason = reason;
        AddDomainEvent(new EnrollmentRejectedEvent(Id, StudentId, CourseId, reason));
    }
}