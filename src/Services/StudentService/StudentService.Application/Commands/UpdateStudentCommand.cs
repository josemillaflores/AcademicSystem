using AcademicSystem.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using StudentService.Domain.Entities;
using StudentService.Domain.Interfaces;
using StudentService.Domain.ValueObjects;

namespace StudentService.Application.Commands;

public record UpdateStudentCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? Address = null
) : IRequest<Result<bool>>;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result<bool>>
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<UpdateStudentCommandHandler> _logger;

    public UpdateStudentCommandHandler(
        IStudentRepository repository,
        ILogger<UpdateStudentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _repository.GetByIdAsync(request.Id, cancellationToken);
            
            if (student == null)
                return Result<bool>.Failure($"Student with ID {request.Id} not found");
            
            var name = new StudentName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            
            student.UpdatePersonalInfo(name, email);
            
            if (!string.IsNullOrEmpty(request.Phone))
            {
                student.UpdateContactInfo(request.Phone, request.Address);
            }
            
            await _repository.UpdateAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Student updated: {StudentId}", request.Id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {StudentId}", request.Id);
            return Result<bool>.Failure(ex.Message);
        }
    }
}