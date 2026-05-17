using AcademicSystem.Common.Results;
using CourseService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CourseService.Application.Commands;

public record RemovePrerequisiteCommand(Guid CourseId, Guid PrerequisiteId) : IRequest<Result<bool>>;

public class RemovePrerequisiteCommandHandler : IRequestHandler<RemovePrerequisiteCommand, Result<bool>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<RemovePrerequisiteCommandHandler> _logger;

    public RemovePrerequisiteCommandHandler(
        ICourseRepository repository,
        ILogger<RemovePrerequisiteCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemovePrerequisiteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
            
            if (course == null)
                return Result<bool>.Failure($"Course with ID {request.CourseId} not found");
            
            course.RemovePrerequisite(request.PrerequisiteId);
            
            await _repository.UpdateAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Prerequisite {PrerequisiteId} removed from course {CourseId}", 
                request.PrerequisiteId, request.CourseId);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing prerequisite from course {CourseId}", request.CourseId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}