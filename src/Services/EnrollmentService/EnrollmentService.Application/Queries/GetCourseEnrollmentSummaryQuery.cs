using MediatR;
using EnrollmentService.Application.DTOs;

namespace EnrollmentService.Application.Queries;

public record GetCourseEnrollmentSummaryQuery() : IRequest<IEnumerable<CourseEnrollmentSummaryDto>>;

public class GetCourseEnrollmentSummaryQueryHandler : IRequestHandler<GetCourseEnrollmentSummaryQuery, IEnumerable<CourseEnrollmentSummaryDto>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetCourseEnrollmentSummaryQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseEnrollmentSummaryDto>> Handle(GetCourseEnrollmentSummaryQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetAllAsync(cancellationToken);
        var enrollmentList = enrollments.ToList();
        
        var summary = enrollmentList
            .GroupBy(e => e.CourseId)
            .Select(g => new CourseEnrollmentSummaryDto(
                CourseId: g.Key,
                CourseName: g.First().CourseName,
                CourseCode: g.First().CourseCode,
                TotalEnrollments: g.Count(),
                ApprovedEnrollments: g.Count(e => e.Status == EnrollmentStatus.Approved),
                RejectedEnrollments: g.Count(e => e.Status == EnrollmentStatus.Rejected),
                PendingEnrollments: g.Count(e => e.Status == EnrollmentStatus.Pending),
                MaxCapacity: 0, // Esto vendría de CourseService
                AvailableSlots: 0,
                EnrollmentPercentage: 0
            ));
        
        return summary;
    }
}