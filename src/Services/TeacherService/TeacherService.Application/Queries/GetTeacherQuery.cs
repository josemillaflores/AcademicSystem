using AutoMapper;
using MediatR;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Queries;

public record GetTeacherQuery(Guid Id) : IRequest<TeacherDto?>;

public class GetTeacherQueryHandler : IRequestHandler<GetTeacherQuery, TeacherDto?>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetTeacherQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TeacherDto?> Handle(GetTeacherQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return teacher == null ? null : _mapper.Map<TeacherDto>(teacher);
    }
}