using MediatR;
using CourseService.Application.DTOs;

namespace CourseService.Application.Queries;

public record GetAllCoursesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    int? MinCredits = null,
    int? MaxCredits = null
) : IRequest<PagedResult<CourseDto>>;

public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, PagedResult<CourseDto>>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public GetAllCoursesQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = await _repository.GetAllAsync(cancellationToken);
        var courseList = courses.ToList();
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<CourseStatus>(request.Status);
            courseList = courseList.Where(c => c.Status == status).ToList();
        }
        
        if (request.MinCredits.HasValue)
            courseList = courseList.Where(c => c.Credits >= request.MinCredits.Value).ToList();
        
        if (request.MaxCredits.HasValue)
            courseList = courseList.Where(c => c.Credits <= request.MaxCredits.Value).ToList();
        
        var totalCount = courseList.Count;
        var items = courseList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
        
        var dtos = _mapper.Map<IEnumerable<CourseDto>>(items);
        
        return new PagedResult<CourseDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}