namespace AcademicSystem.Contracts.Courses;

public record CourseResponse(
    Guid Id,
    string Code,
    string Name,
    string Description,
    int Credits,
    int MaxCapacity,
    int CurrentEnrollment,
    string Status
);

public record CreateCourseRequest(
    string Code,
    string Name,
    string Description,
    int Credits,
    int MaxCapacity
);