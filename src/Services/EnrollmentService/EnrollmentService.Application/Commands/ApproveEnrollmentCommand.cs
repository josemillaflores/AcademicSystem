using AcademicSystem.Common.Results;
using AcademicSystem.EventBus;
using EnrollmentService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace EnrollmentService.Application.Commands;

public record ApproveEnrollmentCommand(Guid EnrollmentId) : IRequest<Result<bool>>;

public class ApproveEnrollmentCommandHandler : IRequestHandler<ApproveEnrollmentCommand, Result<bool>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ApproveEnrollmentCommandHandler> _logger;

    public ApproveEnrollmentCommandHandler(
        IEnrollmentRepository repository,
        IEventBus eventBus,
        ILogger<ApproveEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ApproveEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = await _repository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            
            if (enrollment == null)
                return Result<bool>.Failure($"Enrollment with ID {request.EnrollmentId} not found");
            
            enrollment.Approve();
            
            await _repository.UpdateAsync(enrollment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Publicar evento de matrícula aprobada
            await _eventBus.PublishAsync(new EnrollmentApprovedEvent(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.CourseId
            ));
            
            _logger.LogInformation("Enrollment approved: {EnrollmentId}", request.EnrollmentId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving enrollment {EnrollmentId}", request.EnrollmentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}