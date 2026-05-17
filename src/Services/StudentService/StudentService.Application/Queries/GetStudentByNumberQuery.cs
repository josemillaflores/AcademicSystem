using AutoMapper;
using MediatR;
using StudentService.Application.DTOs;
using StudentService.Domain.Interfaces;

namespace StudentService.Application.Queries;

public record GetStudentByNumberQuery(string StudentNumber) : IRequest<StudentDto?>;

public class GetStudentByNumberQueryHandler : IRequestHandler<GetStudentByNumberQuery, StudentDto?>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentByNumberQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StudentDto?> Handle(GetStudentByNumberQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByStudentNumberAsync(request.StudentNumber, cancellationToken);
        return student == null ? null : _mapper.Map<StudentDto>(student);
    }
}