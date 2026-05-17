using AcademicSystem.Common.Results;
using AcademicSystem.EventBus;
using EnrollmentService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnrollmentService.Application.Commands;

public record RejectEnrollmentCommand(
    Guid EnrollmentId,
    string Reason
) : IRequest<Result<bool>>;

public class RejectEnrollmentCommandHandler : IRequestHandler<RejectEnrollmentCommand, Result<bool>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RejectEnrollmentCommandHandler> _logger;

    public RejectEnrollmentCommandHandler(
        IEnrollmentRepository repository,
        IEventBus eventBus,
        ILogger<RejectEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RejectEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = await _repository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            
            if (enrollment == null)
                return Result<bool>.Failure($"Enrollment with ID {request.EnrollmentId} not found");
            
            enrollment.Reject(request.Reason);
            
            await _repository.UpdateAsync(enrollment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Publicar evento de matrícula rechazada
            await _eventBus.PublishAsync(new EnrollmentRejectedEvent(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.CourseId,
                request.Reason
            ));
            
            _logger.LogInformation("Enrollment rejected: {EnrollmentId}, Reason: {Reason}", 
                request.EnrollmentId, request.Reason);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting enrollment {EnrollmentId}", request.EnrollmentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}