namespace CourseService.Application.DTOs;

/// <summary>
/// DTO para disponibilidad de cupo en un curso
/// </summary>
public record CourseAvailabilityDto(
    Guid CourseId,
    string CourseCode,
    string CourseName,
    int MaxCapacity,
    int CurrentEnrollment,
    int AvailableSlots,
    double EnrollmentPercentage,
    bool HasAvailability,
    string WaitlistStatus
);