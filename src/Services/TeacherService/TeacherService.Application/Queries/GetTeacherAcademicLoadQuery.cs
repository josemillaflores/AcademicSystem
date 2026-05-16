using AutoMapper;
using MediatR;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Queries;

public record GetTeacherAcademicLoadQuery(Guid TeacherId) : IRequest<AcademicLoadDto?>;

public class GetTeacherAcademicLoadQueryHandler : IRequestHandler<GetTeacherAcademicLoadQuery, AcademicLoadDto?>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetTeacherAcademicLoadQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<AcademicLoadDto?> Handle(GetTeacherAcademicLoadQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
        
        if (teacher == null)
            return null;
            
        return _mapper.Map<AcademicLoadDto>(teacher);
    }
}