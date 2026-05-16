using MediatR;
using CourseService.Application.DTOs;

namespace CourseService.Application.Queries;

public record GetCourseByCodeQuery(string Code) : IRequest<CourseDto?>;

public class GetCourseByCodeQueryHandler : IRequestHandler<GetCourseByCodeQuery, CourseDto?>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public GetCourseByCodeQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CourseDto?> Handle(GetCourseByCodeQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        return course == null ? null : _mapper.Map<CourseDto>(course);
    }
}