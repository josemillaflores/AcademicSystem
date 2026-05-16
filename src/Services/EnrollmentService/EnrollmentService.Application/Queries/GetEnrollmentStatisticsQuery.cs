using MediatR;
using EnrollmentService.Application.DTOs;

namespace EnrollmentService.Application.Queries;

public record GetEnrollmentStatisticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<EnrollmentStatisticsDto>;

public class GetEnrollmentStatisticsQueryHandler : IRequestHandler<GetEnrollmentStatisticsQuery, EnrollmentStatisticsDto>
{
    private readonly IEnrollmentRepository _repository;

    public GetEnrollmentStatisticsQueryHandler(IEnrollmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<EnrollmentStatisticsDto> Handle(GetEnrollmentStatisticsQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetAllAsync(cancellationToken);
        var enrollmentList = enrollments.ToList();
        
        if (request.StartDate.HasValue)
            enrollmentList = enrollmentList.Where(e => e.CreatedAt >= request.StartDate.Value).ToList();
        
        if (request.EndDate.HasValue)
            enrollmentList = enrollmentList.Where(e => e.CreatedAt <= request.EndDate.Value).ToList();
        
        var pendingEnrollments = enrollmentList.Count(e => e.Status == EnrollmentStatus.Pending);
        var approvedEnrollments = enrollmentList.Count(e => e.Status == EnrollmentStatus.Approved);
        var rejectedEnrollments = enrollmentList.Count(e => e.Status == EnrollmentStatus.Rejected);
        var cancelledEnrollments = enrollmentList.Count(e => e.Status == EnrollmentStatus.Cancelled);
        
        var approvalRate = enrollmentList.Count > 0 
            ? (double)approvedEnrollments / enrollmentList.Count * 100 
            : 0;
        
        var enrollmentsThisMonth = enrollmentList.Count(e => e.CreatedAt.Month == DateTime.UtcNow.Month);
        
        return new EnrollmentStatisticsDto(
            TotalEnrollments: enrollmentList.Count,
            PendingEnrollments: pendingEnrollments,
            ApprovedEnrollments: approvedEnrollments,
            RejectedEnrollments: rejectedEnrollments,
            CancelledEnrollments: cancelledEnrollments,
            ApprovalRate: approvalRate,
            EnrollmentsThisMonth: enrollmentsThisMonth,
            EnrollmentsByStatus: new Dictionary<string, int>
            {
                ["Pending"] = pendingEnrollments,
                ["Approved"] = approvedEnrollments,
                ["Rejected"] = rejectedEnrollments,
                ["Cancelled"] = cancelledEnrollments
            },
            EnrollmentsByPeriod: new Dictionary<string, int>()
        );
    }
}