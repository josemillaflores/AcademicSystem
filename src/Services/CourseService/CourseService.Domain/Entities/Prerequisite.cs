namespace CourseService.Domain.Entities;

public class Prerequisite : BaseEntity
{
    public Guid CourseId { get; private set; }
    public Guid RequiredCourseId { get; private set; }
    public string RequiredCourseName { get; private set; }
    public string RequiredCourseCode { get; private set; }
    public bool IsMandatory { get; private set; }

    private Prerequisite() { }

    public Prerequisite(Guid requiredCourseId, string requiredCourseName, bool isMandatory)
    {
        RequiredCourseId = requiredCourseId;
        RequiredCourseName = requiredCourseName;
        IsMandatory = isMandatory;
    }

    public void UpdateRequiredCourseInfo(string name, string code)
    {
        RequiredCourseName = name;
        RequiredCourseCode = code;
        UpdateTimestamp();
    }
}