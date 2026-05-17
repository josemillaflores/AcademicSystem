using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using StudentService.Domain.Interfaces;

namespace StudentService.Application.Commands;

public record DeleteStudentCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Result<bool>>
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<DeleteStudentCommandHandler> _logger;

    public DeleteStudentCommandHandler(
        IStudentRepository repository,
        ILogger<DeleteStudentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (student == null)
                return Result<bool>.Failure($"Student with ID {request.Id} not found");
            
            await _repository.DeleteAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Student deleted: {StudentId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {StudentId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}