using AutoMapper;
using MediatR;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Queries;

public record GetTeacherCoursesQuery(Guid TeacherId) : IRequest<IEnumerable<TeacherCourseDto>>;

public class GetTeacherCoursesQueryHandler : IRequestHandler<GetTeacherCoursesQuery, IEnumerable<TeacherCourseDto>>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetTeacherCoursesQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeacherCourseDto>> Handle(GetTeacherCoursesQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
        
        if (teacher == null)
            return Enumerable.Empty<TeacherCourseDto>();
            
        return _mapper.Map<IEnumerable<TeacherCourseDto>>(teacher.CourseAssignments);
    }
}