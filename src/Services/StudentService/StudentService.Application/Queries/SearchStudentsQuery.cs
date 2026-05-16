using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Queries;

public record SearchStudentsQuery(string SearchTerm) : IRequest<IEnumerable<StudentDto>>;

public class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, IEnumerable<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public SearchStudentsQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentDto>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.FindAsync(s => 
            s.Name.FirstName.Contains(request.SearchTerm) ||
            s.Name.LastName.Contains(request.SearchTerm) ||
            s.Email.Value.Contains(request.SearchTerm) ||
            s.StudentNumber.Value.Contains(request.SearchTerm),
            cancellationToken);
            
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }
}