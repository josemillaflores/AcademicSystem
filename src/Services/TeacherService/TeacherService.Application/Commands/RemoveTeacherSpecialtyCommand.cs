using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Commands;

public record RemoveTeacherSpecialtyCommand(Guid TeacherId, Guid SpecialtyId) : IRequest<Result<bool>>;

public class RemoveTeacherSpecialtyCommandHandler : IRequestHandler<RemoveTeacherSpecialtyCommand, Result<bool>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<RemoveTeacherSpecialtyCommandHandler> _logger;

    public RemoveTeacherSpecialtyCommandHandler(
        ITeacherRepository repository,
        ILogger<RemoveTeacherSpecialtyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RemoveTeacherSpecialtyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
            
            if (teacher == null)
                return Result<bool>.Failure($"Teacher with ID {request.TeacherId} not found");
            
            teacher.RemoveSpecialty(request.SpecialtyId);
            
            await _repository.UpdateAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Specialty {SpecialtyId} removed from teacher {TeacherId}", 
                request.SpecialtyId, request.TeacherId);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing specialty from teacher {TeacherId}", request.TeacherId);
            return Result<bool>.Failure(ex.Message);
        }
    }
}