using MediatR;
using CourseService.Application.DTOs;

namespace CourseService.Application.Queries;

public record CheckCourseAvailabilityQuery(Guid CourseId) : IRequest<CourseAvailabilityDto?>;

public class CheckCourseAvailabilityQueryHandler : IRequestHandler<CheckCourseAvailabilityQuery, CourseAvailabilityDto?>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public CheckCourseAvailabilityQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CourseAvailabilityDto?> Handle(CheckCourseAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
        
        if (course == null)
            return null;
            
        return _mapper.Map<CourseAvailabilityDto>(course);
    }
}