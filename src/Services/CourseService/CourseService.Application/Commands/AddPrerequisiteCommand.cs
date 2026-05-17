using AcademicSystem.Common.Results;
using MediatR;

namespace CourseService.Application.Commands;

public record AddPrerequisiteCommand(
    Guid CourseId,
    Guid RequiredCourseId,
    bool IsMandatory = true
) : IRequest<Result<Guid>>;

public class AddPrerequisiteCommandHandler : IRequestHandler<AddPrerequisiteCommand, Result<Guid>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<AddPrerequisiteCommandHandler> _logger;

    public AddPrerequisiteCommandHandler(
        ICourseRepository repository,
        ILogger<AddPrerequisiteCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddPrerequisiteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
            
            if (course == null)
                return Result<Guid>.Failure($"Course with ID {request.CourseId} not found");
            
            var requiredCourse = await _repository.GetByIdAsync(request.RequiredCourseId, cancellationToken);
            
            if (requiredCourse == null)
                return Result<Guid>.Failure($"Required course with ID {request.RequiredCourseId} not found");
            
            var prerequisite = course.AddPrerequisite(request.RequiredCourseId, requiredCourse.Name, request.IsMandatory);
            
            await _repository.UpdateAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Prerequisite {RequiredCourseId} added to course {CourseId}", 
                request.RequiredCourseId, request.CourseId);
            
            return Result<Guid>.Success(prerequisite.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding prerequisite to course {CourseId}", request.CourseId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}