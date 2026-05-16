using MediatR;

namespace StudentService.Application.Commands;

public record EnrollStudentCommand(
    Guid StudentId,
    Guid CourseId,
    string CourseName,
    int Credits
) : IRequest<Result<Guid>>;

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<EnrollStudentCommandHandler> _logger;

    public EnrollStudentCommandHandler(
        IStudentRepository repository,
        ILogger<EnrollStudentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
            
            if (student == null)
                return Result<Guid>.Failure($"Student with ID {request.StudentId} not found");
            
            student.EnrollInCourse(request.CourseId, request.CourseName, request.Credits);
            
            await _repository.UpdateAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            var enrollmentId = student.Enrollments.First(e => e.CourseId == request.CourseId).Id;
            
            _logger.LogInformation("Student {StudentId} enrolled in course {CourseId}", 
                request.StudentId, request.CourseId);
                
            return Result<Guid>.Success(enrollmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student {StudentId} in course {CourseId}", 
                request.StudentId, request.CourseId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}