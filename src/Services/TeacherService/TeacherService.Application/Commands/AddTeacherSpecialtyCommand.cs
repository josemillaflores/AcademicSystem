using AcademicSystem.Common.Results;
using MediatR;
using TeacherService.Domain.Entities;

namespace TeacherService.Application.Commands;

public record AddTeacherSpecialtyCommand(
    Guid TeacherId,
    string SpecialtyName,
    string? Description = null
) : IRequest<Result<Guid>>;

public class AddTeacherSpecialtyCommandHandler : IRequestHandler<AddTeacherSpecialtyCommand, Result<Guid>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<AddTeacherSpecialtyCommandHandler> _logger;

    public AddTeacherSpecialtyCommandHandler(
        ITeacherRepository repository,
        ILogger<AddTeacherSpecialtyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddTeacherSpecialtyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
            
            if (teacher == null)
                return Result<Guid>.Failure($"Teacher with ID {request.TeacherId} not found");
            
            var specialty = new Specialty(request.SpecialtyName, request.Description);
            teacher.AddSpecialty(specialty);
            
            await _repository.UpdateAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Specialty '{Specialty}' added to teacher {TeacherId}", 
                request.SpecialtyName, request.TeacherId);
            
            return Result<Guid>.Success(specialty.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding specialty to teacher {TeacherId}", request.TeacherId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}