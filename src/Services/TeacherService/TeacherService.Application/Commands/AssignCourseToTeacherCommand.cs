using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Commands;

public record AssignCourseToTeacherCommand(
    Guid TeacherId,
    Guid CourseId,
    string CourseName,
    int HoursPerWeek,
    string? Period = null
) : IRequest<Result<Guid>>;

public class AssignCourseToTeacherCommandHandler : IRequestHandler<AssignCourseToTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<AssignCourseToTeacherCommandHandler> _logger;

    public AssignCourseToTeacherCommandHandler(
        ITeacherRepository repository,
        ILogger<AssignCourseToTeacherCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AssignCourseToTeacherCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
            
            if (teacher == null)
                return Result<Guid>.Failure($"Teacher with ID {request.TeacherId} not found");
            
            var assignment = teacher.AssignCourse(request.CourseId, request.CourseName, request.HoursPerWeek, request.Period);
            
            await _repository.UpdateAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Course {CourseId} assigned to teacher {TeacherId}", 
                request.CourseId, request.TeacherId);
            
            return Result<Guid>.Success(assignment.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot assign course to teacher {TeacherId}", request.TeacherId);
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning course to teacher {TeacherId}", request.TeacherId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}