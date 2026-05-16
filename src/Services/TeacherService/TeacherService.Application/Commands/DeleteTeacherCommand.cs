using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Commands;

public record DeleteTeacherCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, Result<bool>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<DeleteTeacherCommandHandler> _logger;

    public DeleteTeacherCommandHandler(
        ITeacherRepository repository,
        ILogger<DeleteTeacherCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (teacher == null)
                return Result<bool>.Failure($"Teacher with ID {request.Id} not found");
            
            await _repository.DeleteAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Teacher deleted: {TeacherId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher {TeacherId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}