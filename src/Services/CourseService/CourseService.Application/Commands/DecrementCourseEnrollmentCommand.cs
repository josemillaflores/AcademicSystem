using AcademicSystem.Common.Results;
using MediatR;

namespace CourseService.Application.Commands;

public record DecrementCourseEnrollmentCommand(Guid CourseId) : IRequest<Result<int>>;

public class DecrementCourseEnrollmentCommandHandler : IRequestHandler<DecrementCourseEnrollmentCommand, Result<int>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<DecrementCourseEnrollmentCommandHandler> _logger;

    public DecrementCourseEnrollmentCommandHandler(
        ICourseRepository repository,
        ILogger<DecrementCourseEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(DecrementCourseEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
            
            if (course == null)
                return Result<int>.Failure($"Course with ID {request.CourseId} not found");
            
            course.DecrementEnrollment();
            
            await _repository.UpdateAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Enrollment decremented for course {CourseId}. New count: {Count}", 
                request.CourseId, course.CurrentEnrollment);
            
            return Result<int>.Success(course.CurrentEnrollment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing enrollment for course {CourseId}", request.CourseId);
            return Result<int>.Failure(ex.Message);
        }
    }
}