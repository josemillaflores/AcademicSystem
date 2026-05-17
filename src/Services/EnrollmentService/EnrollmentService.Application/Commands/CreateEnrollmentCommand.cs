using AcademicSystem.Common.Results;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnrollmentService.Application.Commands;

public record CreateEnrollmentCommand(
    Guid StudentId,
    Guid CourseId,
    string? Period = null
) : IRequest<Result<Guid>>;

public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, Result<Guid>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly ILogger<CreateEnrollmentCommandHandler> _logger;

    public CreateEnrollmentCommandHandler(
        IEnrollmentRepository repository,
        ILogger<CreateEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var period = request.Period ?? GetCurrentPeriod();
            var enrollment = new Enrollment(
                request.StudentId,
                request.CourseId,
                period
            );
            
            await _repository.AddAsync(enrollment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Enrollment created with ID: {EnrollmentId} for Student {StudentId}, Course {CourseId}", 
                enrollment.Id, request.StudentId, request.CourseId);
            
            return Result<Guid>.Success(enrollment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enrollment");
            return Result<Guid>.Failure(ex.Message);
        }
    }
    
    private string GetCurrentPeriod()
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var semester = now.Month <= 7 ? "1" : "2";
        return $"{year}-{semester}";
    }
}