using MediatR;
using CourseService.Application.DTOs;
using CourseService.Domain.Interfaces;
using AutoMapper;

namespace CourseService.Application.Queries;

public record GetCourseQuery(Guid Id) : IRequest<CourseDto?>;

public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, CourseDto?>
{
    private readonly ICourseRepository _repository;
    private readonly IMapper _mapper;

    public GetCourseQueryHandler(ICourseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CourseDto?> Handle(GetCourseQuery request, CancellationToken cancellationToken)
    {
        var course = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return course == null ? null : _mapper.Map<CourseDto>(course);
    }
}