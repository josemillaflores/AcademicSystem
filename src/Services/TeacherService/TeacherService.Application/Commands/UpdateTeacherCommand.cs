using AcademicSystem.Common.Results;
using MediatR;
using TeacherService.Domain.ValueObjects;

namespace TeacherService.Application.Commands;

public record UpdateTeacherCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
) : IRequest<Result<bool>>;

public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, Result<bool>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<UpdateTeacherCommandHandler> _logger;

    public UpdateTeacherCommandHandler(
        ITeacherRepository repository,
        ILogger<UpdateTeacherCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (teacher == null)
                return Result<bool>.Failure($"Teacher with ID {request.Id} not found");
            
            var name = new TeacherName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            
            teacher.UpdatePersonalInfo(name, email);
            
            await _repository.UpdateAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Teacher updated: {TeacherId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher {TeacherId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}