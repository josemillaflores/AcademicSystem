using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Queries;

public record GetStudentQuery(Guid Id) : IRequest<StudentDto?>;

public class GetStudentQueryHandler : IRequestHandler<GetStudentQuery, StudentDto?>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StudentDto?> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.Id);
        return student == null ? null : _mapper.Map<StudentDto>(student);
    }
}