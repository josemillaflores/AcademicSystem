using MediatR;
using CourseService.Application.DTOs;
using CourseService.Domain.Interfaces;

namespace CourseService.Application.Queries;

public record GetCourseStatisticsQuery() : IRequest<CourseStatisticsDto>;

public class GetCourseStatisticsQueryHandler : IRequestHandler<GetCourseStatisticsQuery, CourseStatisticsDto>
{
    private readonly ICourseRepository _repository;

    public GetCourseStatisticsQueryHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<CourseStatisticsDto> Handle(GetCourseStatisticsQuery request, CancellationToken cancellationToken)
    {
        var courses = await _repository.GetAllAsync(cancellationToken);
        var courseList = courses.ToList();
        
        var activeCourses = courseList.Count(c => c.Status == CourseStatus.Active);
        var fullCourses = courseList.Count(c => c.Status == CourseStatus.Full);
        var cancelledCourses = courseList.Count(c => c.Status == CourseStatus.Cancelled);
        
        var totalEnrollments = courseList.Sum(c => c.CurrentEnrollment);
        var totalCapacity = courseList.Sum(c => c.MaxCapacity);
        var averageEnrollmentRate = totalCapacity > 0 ? (double)totalEnrollments / totalCapacity * 100 : 0;
        
        return new CourseStatisticsDto(
            TotalCourses: courseList.Count,
            ActiveCourses: activeCourses,
            FullCourses: fullCourses,
            CancelledCourses: cancelledCourses,
            AverageCredits: courseList.Average(c => c.Credits),
            TotalEnrollments: totalEnrollments,
            AverageEnrollmentRate: averageEnrollmentRate,
            TotalRevenue: totalEnrollments * 500, // Asumiendo $500 por curso
            CoursesByDepartment: new Dictionary<string, int>(),
            CoursesByStatus: new Dictionary<string, int>
            {
                ["Active"] = activeCourses,
                ["Full"] = fullCourses,
                ["Cancelled"] = cancelledCourses
            }
        );
    }
}