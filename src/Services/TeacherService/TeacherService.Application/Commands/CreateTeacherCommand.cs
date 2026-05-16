using MediatR;

namespace TeacherService.Application.Commands;

public record CreateTeacherCommand(
    string FirstName,
    string LastName,
    string Email,
    DateTime HireDate
) : IRequest<Result<Guid>>;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    private readonly ITeacherRepository _repository;
    private readonly ILogger<CreateTeacherCommandHandler> _logger;

    public CreateTeacherCommandHandler(
        ITeacherRepository repository,
        ILogger<CreateTeacherCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var name = new TeacherName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            var teacherNumber = new TeacherId($"TCH{DateTime.Now:yyyy}{new Random().Next(1000, 9999)}");
            
            var teacher = new Teacher(name, email, teacherNumber, request.HireDate);
            
            await _repository.AddAsync(teacher, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Teacher created with ID: {TeacherId}, Number: {TeacherNumber}", 
                teacher.Id, teacherNumber.Value);
            
            return Result<Guid>.Success(teacher.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}