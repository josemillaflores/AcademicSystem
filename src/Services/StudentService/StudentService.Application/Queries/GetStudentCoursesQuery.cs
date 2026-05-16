using MediatR;
using StudentService.Application.DTOs;

namespace StudentService.Application.Queries;

public record GetStudentCoursesQuery(Guid StudentId) : IRequest<IEnumerable<StudentCourseDto>>;

public class GetStudentCoursesQueryHandler : IRequestHandler<GetStudentCoursesQuery, IEnumerable<StudentCourseDto>>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentCoursesQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentCourseDto>> Handle(GetStudentCoursesQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
        
        if (student == null)
            return Enumerable.Empty<StudentCourseDto>();
            
        return _mapper.Map<IEnumerable<StudentCourseDto>>(student.Enrollments);
    }
}