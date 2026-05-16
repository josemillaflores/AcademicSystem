using MediatR;
using CourseService.Application.DTOs;

namespace CourseService.Application.Queries;

public record GetCoursePrerequisitesQuery(Guid CourseId) : IRequest<IEnumerable<PrerequisiteDto>>;

public class GetCoursePrerequisitesQueryHandler : IRequestHandler<GetCoursePrerequisitesQuery, IEnumerable<PrerequisiteDto>>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public GetCoursePrerequisitesQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PrerequisiteDto>> Handle(GetCoursePrerequisitesQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
        
        if (course == null)
            return Enumerable.Empty<PrerequisiteDto>();
            
        return _mapper.Map<IEnumerable<PrerequisiteDto>>(course.Prerequisites);
    }
}