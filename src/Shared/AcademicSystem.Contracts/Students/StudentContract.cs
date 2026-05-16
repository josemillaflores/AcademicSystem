namespace AcademicSystem.Contracts.Students;

public record StudentResponse(
    Guid Id,
    string FullName,
    string Email,
    string StudentNumber,
    DateTime EnrollmentDate,
    string Status
);

public record CreateStudentRequest(
    string FirstName,
    string LastName,
    string Email
);

public record UpdateStudentRequest(
    string FirstName,
    string LastName,
    string Email
);