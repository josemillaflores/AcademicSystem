using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Commands;

public record CreateStudentCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? Address = null
) : IRequest<Result<Guid>>;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateStudentCommandHandler> _logger;

    public CreateStudentCommandHandler(
        IStudentRepository repository,
        IMapper mapper,
        ILogger<CreateStudentCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var name = new StudentName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            var studentNumber = new StudentId(Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper());
            
            var student = new Student(name, email, studentNumber);
            
            if (!string.IsNullOrEmpty(request.Phone))
            {
                student.UpdateContactInfo(request.Phone, request.Address);
            }
            
            await _repository.AddAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Student created with ID: {StudentId}, Number: {StudentNumber}", 
                student.Id, studentNumber.Value);
            
            return Result<Guid>.Success(student.Id, studentNumber.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return Result<Guid>.Failure(ex.Message);
        }
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public string? StudentNumber { get; }
    
    private Result(bool isSuccess, T? data, string? error = null, string? studentNumber = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StudentNumber = studentNumber;
    }
    
    public static Result<T> Success(T data, string? studentNumber = null) => new(true, data, null, studentNumber);
    public static Result<T> Failure(string error) => new(false, default, error);
}