
public class Teacher
{
    public Guid Id { get; private set; }
    public TeacherName Name { get; private set; }
    public Email Email { get; private set; }
    public TeacherId TeacherNumber { get; private set; }
    public DateTime HireDate { get; private set; }
    public TeacherStatus Status { get; private set; }
    public List<Specialty> Specialties { get; private set; }
    public List<CourseAssignment> CourseAssignments { get; private set; }
    public AcademicLoad AcademicLoad { get; private set; }
}

 