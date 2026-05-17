using CourseService.Domain.Interfaces;
using MediatR;

namespace CourseService.Application.Queries;

public record CheckCourseCodeExistsQuery(string Code) : IRequest<bool>;

public class CheckCourseCodeExistsQueryHandler : IRequestHandler<CheckCourseCodeExistsQuery, bool>
{
    private readonly ICourseRepository _repository;

    public CheckCourseCodeExistsQueryHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckCourseCodeExistsQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        return course != null;
    }
}