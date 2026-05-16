using MediatR;
using TeacherService.Application.DTOs;

namespace TeacherService.Application.Queries;

public record GetTeacherStatisticsQuery() : IRequest<TeacherStatisticsDto>;

public class GetTeacherStatisticsQueryHandler : IRequestHandler<GetTeacherStatisticsQuery, TeacherStatisticsDto>
{
    private readonly ITeacherRepository _repository;

    public GetTeacherStatisticsQueryHandler(ITeacherRepository repository)
    {
        _repository = repository;
    }

    public async Task<TeacherStatisticsDto> Handle(GetTeacherStatisticsQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _repository.GetAllAsync(cancellationToken);
        var teacherList = teachers.ToList();
        
        var activeTeachers = teacherList.Count(t => t.Status == TeacherStatus.Active);
        var onLeaveTeachers = teacherList.Count(t => t.Status == TeacherStatus.OnLeave);
        var retiredTeachers = teacherList.Count(t => t.Status == TeacherStatus.Retired);
        
        var averageYearsOfService = teacherList
            .Where(t => t.Status == TeacherStatus.Active)
            .Average(t => (DateTime.UtcNow.Year - t.HireDate.Year));
        
        var specialtiesMap = teacherList
            .SelectMany(t => t.Specialties)
            .GroupBy(s => s.Name)
            .ToDictionary(g => g.Key, g => g.Count());
        
        return new TeacherStatisticsDto(
            TotalTeachers: teacherList.Count,
            ActiveTeachers: activeTeachers,
            OnLeaveTeachers: onLeaveTeachers,
            RetiredTeachers: retiredTeachers,
            AverageYearsOfService: averageYearsOfService,
            NewTeachersThisYear: teacherList.Count(t => t.HireDate.Year == DateTime.UtcNow.Year),
            TeachersBySpecialty: specialtiesMap,
            TeachersByStatus: new Dictionary<string, int>
            {
                ["Active"] = activeTeachers,
                ["OnLeave"] = onLeaveTeachers,
                ["Retired"] = retiredTeachers
            }
        );
    }
}