using AcademicSystem.Common.Entities;

namespace CourseService.Domain.Entities;

public class Prerequisite : BaseEntity
{
    public Guid CourseId { get; private set; }
    public Guid RequiredCourseId { get; private set; }
    public string RequiredCourseName { get; private set; }
    public string RequiredCourseCode { get; private set; }
    public bool IsMandatory { get; private set; }

    private Prerequisite() { }

    public Prerequisite(Guid requiredCourseId, string requiredCourseName, string requiredCourseCode, bool isMandatory)
    {
        RequiredCourseId = requiredCourseId;
        RequiredCourseName = requiredCourseName;
        RequiredCourseCode = requiredCourseCode;
        IsMandatory = isMandatory;
    }
}