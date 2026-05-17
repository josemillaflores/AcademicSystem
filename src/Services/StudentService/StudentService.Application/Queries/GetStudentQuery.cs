using AutoMapper;
using MediatR;
using StudentService.Application.DTOs;
using StudentService.Domain.Interfaces;

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
        var student = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return student == null ? null : _mapper.Map<StudentDto>(student);
    }
}