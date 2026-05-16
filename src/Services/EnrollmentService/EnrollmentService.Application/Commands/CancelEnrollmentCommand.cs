using MediatR;

namespace EnrollmentService.Application.Commands;

public record CancelEnrollmentCommand(Guid EnrollmentId) : IRequest<Result<bool>>;

public class CancelEnrollmentCommandHandler : IRequestHandler<CancelEnrollmentCommand, Result<bool>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CancelEnrollmentCommandHandler> _logger;

    public CancelEnrollmentCommandHandler(
        IEnrollmentRepository repository,
        IEventBus eventBus,
        ILogger<CancelEnrollmentCommandHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var enrollment = await _repository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            
            if (enrollment == null)
                return Result<bool>.Failure($"Enrollment with ID {request.EnrollmentId} not found");
            
            enrollment.Cancel();
            
            await _repository.UpdateAsync(enrollment, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Publicar evento de matrícula cancelada
            await _eventBus.PublishAsync(new EnrollmentCancelledEvent(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.CourseId
            ));
            
            _logger.LogInformation("Enrollment cancelled: {EnrollmentId}", request.EnrollmentId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling enrollment {EnrollmentId}", request.EnrollmentId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}