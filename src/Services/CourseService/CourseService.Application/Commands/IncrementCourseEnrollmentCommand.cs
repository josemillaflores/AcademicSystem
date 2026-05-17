using AcademicSystem.Common.Results;
using CourseService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CourseService.Application.Commands;

public record IncrementCourseEnrollmentCommand(Guid CourseId) : IRequest<Result<int>>;

public class IncrementCourseEnrollmentCommandHandler : IRequestHandler<IncrementCourseEnrollmentCommand, Result<int>>
{
    private readonly ICourseRepository _repository;
    private readonly ILogger<IncrementCourseEnrollmentCommandHandler> _logger;

    public IncrementCourseEnrollmentCommandHandler(
        ICourseRepository repository,
        ILogger<IncrementCourseEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(IncrementCourseEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _repository.GetByIdAsync(request.CourseId, cancellationToken);
            
            if (course == null)
                return Result<int>.Failure($"Course with ID {request.CourseId} not found");
            
            var result = course.IncrementEnrollment();
            
            if (!result)
                return Result<int>.Failure("Course is already full");
            
            await _repository.UpdateAsync(course, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Enrollment incremented for course {CourseId}. New count: {Count}", 
                request.CourseId, course.CurrentEnrollment);
            
            return Result<int>.Success(course.CurrentEnrollment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing enrollment for course {CourseId}", request.CourseId);
            return Result<int>.Failure(ex.Message);
        }
    }
}