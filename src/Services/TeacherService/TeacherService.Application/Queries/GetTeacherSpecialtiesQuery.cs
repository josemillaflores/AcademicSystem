using AutoMapper;
using MediatR;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Interfaces;

namespace TeacherService.Application.Queries;

public record GetTeacherSpecialtiesQuery(Guid TeacherId) : IRequest<IEnumerable<SpecialtyDto>>;

public class GetTeacherSpecialtiesQueryHandler : IRequestHandler<GetTeacherSpecialtiesQuery, IEnumerable<SpecialtyDto>>
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;

    public GetTeacherSpecialtiesQueryHandler(ITeacherRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpecialtyDto>> Handle(GetTeacherSpecialtiesQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _repository.GetByIdAsync(request.TeacherId, cancellationToken);
        
        if (teacher == null)
            return Enumerable.Empty<SpecialtyDto>();
            
        return _mapper.Map<IEnumerable<SpecialtyDto>>(teacher.Specialties);
    }
}