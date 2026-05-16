using MediatR;
using TeacherService.Application.DTOs;

namespace TeacherService.Application.Queries;

public record GetAllTeachersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<TeacherDto>>;

public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, PagedResult<TeacherDto>>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTeachersQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TeacherDto>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _repository.GetAllAsync(cancellationToken);
        var teacherList = teachers.ToList();
        
        var totalCount = teacherList.Count;
        var items = teacherList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
        
        var dtos = _mapper.Map<IEnumerable<TeacherDto>>(items);
        
        return new PagedResult<TeacherDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}