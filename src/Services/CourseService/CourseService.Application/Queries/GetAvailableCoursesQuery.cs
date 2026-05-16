using MediatR;
using CourseService.Application.DTOs;

namespace CourseService.Application.Queries;

public record GetAvailableCoursesQuery() : IRequest<IEnumerable<CourseDto>>;

public class GetAvailableCoursesQueryHandler : IRequestHandler<GetAvailableCoursesQuery, IEnumerable<CourseDto>>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public GetAvailableCoursesQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> Handle(GetAvailableCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await _repository.GetAvailableCoursesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }
}