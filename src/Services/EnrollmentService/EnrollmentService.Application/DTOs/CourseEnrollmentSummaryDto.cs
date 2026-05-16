namespace EnrollmentService.Application.DTOs;

/// <summary>
/// DTO para resumen de matrículas por curso
/// </summary>
public record CourseEnrollmentSummaryDto(
    Guid CourseId,
    string CourseName,
    string CourseCode,
    int TotalEnrollments,
    int ApprovedEnrollments,
    int RejectedEnrollments,
    int PendingEnrollments,
    int MaxCapacity,
    int AvailableSlots,
    double EnrollmentPercentage
);