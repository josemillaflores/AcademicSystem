using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Queries;

public record GetStudentByEmailQuery(string Email) : IRequest<StudentDto?>;

public class GetStudentByEmailQueryHandler : IRequestHandler<GetStudentByEmailQuery, StudentDto?>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentByEmailQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StudentDto?> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.FindAsync(s => s.Email.Value == request.Email, cancellationToken);
        var student = students.FirstOrDefault();
        return student == null ? null : _mapper.Map<StudentDto>(student);
    }
}