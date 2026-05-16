using MediatR;

namespace CourseService.Application.Commands;

public record UpdateCourseCommand(
    Guid Id,
    string Name,
    string Description,
    int Credits,
    int MaxCapacity
) : IRequest<Result<bool>>;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Result<bool>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<UpdateCourseCommandHandler> _logger;

    public UpdateCourseCommandHandler(
        ICourseRepository repository,
        ILogger<UpdateCourseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (course == null)
                return Result<bool>.Failure($"Course with ID {request.Id} not found");
            
            course.UpdateInfo(request.Name, request.Description, request.Credits, request.MaxCapacity);
            
            await _repository.UpdateAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Course updated: {CourseId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}