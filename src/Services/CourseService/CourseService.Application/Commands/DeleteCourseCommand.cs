using AcademicSystem.Common.Results;
using MediatR;

namespace CourseService.Application.Commands;

public record DeleteCourseCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Result<bool>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<DeleteCourseCommandHandler> _logger;

    public DeleteCourseCommandHandler(
        ICourseRepository repository,
        ILogger<DeleteCourseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (course == null)
                return Result<bool>.Failure($"Course with ID {request.Id} not found");
            
            await _repository.DeleteAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Course deleted: {CourseId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}