using AutoMapper;
using MediatR;
using StudentService.Application.DTOs;
using StudentService.Domain.Enums;
using StudentService.Domain.Interfaces;

namespace StudentService.Application.Queries;

public record GetAllStudentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null
) : IRequest<PagedResult<StudentDto>>;

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, PagedResult<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetAllStudentsQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.GetAllAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<EnrollmentStatus>(request.Status);
            students = students.Where(s => s.Status == status);
        }
        
        var totalCount = students.Count();
        var items = students
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        var dtos = _mapper.Map<IEnumerable<StudentDto>>(items);
        
        return new PagedResult<StudentDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}