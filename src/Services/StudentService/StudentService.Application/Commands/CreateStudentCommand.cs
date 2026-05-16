using MediatR;

namespace StudentService.Application.Commands;

public record CreateStudentCommand(string FirstName, string LastName, string Email) 
    : IRequest<Guid>;

// Handler
public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly IStudentRepository _repository;

    public CreateStudentCommandHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var name = new StudentName(request.FirstName, request.LastName);
        var email = new Email(request.Email);
        var studentNumber = new StudentId(Guid.NewGuid().ToString("N").Substring(0, 8));
        
        var student = new Student(name, email, studentNumber);
        
        await _repository.AddAsync(student);
        await _repository.SaveChangesAsync();
        
        return student.Id;
    }
}