using AcademicSystem.Common;

namespace TeacherService.Domain.Entities;

public class Specialty : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid TeacherId { get; private set; }

    private Specialty() { }

    public Specialty(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}