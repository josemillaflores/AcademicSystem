using AutoMapper;
using MediatR;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Queries;

public record GetTeacherByNumberQuery(string TeacherNumber) : IRequest<TeacherDto?>;

public class GetTeacherByNumberQueryHandler : IRequestHandler<GetTeacherByNumberQuery, TeacherDto?>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetTeacherByNumberQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TeacherDto?> Handle(GetTeacherByNumberQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _repository.GetByTeacherNumberAsync(request.TeacherNumber, cancellationToken);
        return teacher == null ? null : _mapper.Map<TeacherDto>(teacher);
    }
}