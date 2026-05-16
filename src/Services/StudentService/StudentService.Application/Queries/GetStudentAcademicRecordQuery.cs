using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Queries;

public record GetStudentAcademicRecordQuery(Guid StudentId) : IRequest<AcademicRecordDto?>;

public class GetStudentAcademicRecordQueryHandler : IRequestHandler<GetStudentAcademicRecordQuery, AcademicRecordDto?>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentAcademicRecordQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AcademicRecordDto?> Handle(GetStudentAcademicRecordQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
        
        if (student == null)
            return null;
            
        return _mapper.Map<AcademicRecordDto>(student);
    }
}