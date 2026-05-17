using AcademicSystem.Common.Results;
using MediatR;

namespace CourseService.Application.Commands;

public record CreateCourseCommand(
    string Code,
    string Name,
    string Description,
    int Credits,
    int TotalHours,
    int MaxCapacity
) : IRequest<Result<Guid>>;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<Guid>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<CreateCourseCommandHandler> _logger;

    public CreateCourseCommandHandler(
        ICourseRepository repository,
        ILogger<CreateCourseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = new Course(
                request.Code,
                request.Name,
                request.Description,
                request.Credits,
                request.TotalHours,
                request.MaxCapacity
            );
            
            await _repository.AddAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Course created with ID: {CourseId}, Code: {Code}", 
                course.Id, request.Code);
            
            return Result<Guid>.Success(course.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}