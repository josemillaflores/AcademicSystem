namespace AcademicSystem.Contracts.Enrollments;

public record EnrollmentResponse(
    Guid Id,
    Guid StudentId,
    Guid CourseId,
    DateTime EnrollmentDate,
    string Status,
    string? RejectionReason
);

public record CreateEnrollmentRequest(
    Guid StudentId,
    Guid CourseId
);