public class Prerequisite
{
    public Guid Id { get; private set; }
    public Guid CourseId { get; private set; }
    public Guid RequiredCourseId { get; private set; }
    public bool IsMandatory { get; private set; }
}