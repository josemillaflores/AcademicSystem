namespace EnrollmentService.Application.DTOs;

/// <summary>
/// DTO para estadísticas de matrículas
/// </summary>
public record EnrollmentStatisticsDto(
    int TotalEnrollments,
    int PendingEnrollments,
    int ApprovedEnrollments,
    int RejectedEnrollments,
    int CancelledEnrollments,
    double ApprovalRate,
    int EnrollmentsThisMonth,
    Dictionary<string, int> EnrollmentsByStatus,
    Dictionary<string, int> EnrollmentsByPeriod
);