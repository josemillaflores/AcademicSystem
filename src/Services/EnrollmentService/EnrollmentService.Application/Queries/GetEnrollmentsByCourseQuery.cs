using MediatR;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Domain.Interfaces;
using AutoMapper;

namespace EnrollmentService.Application.Queries;

public record GetEnrollmentsByCourseQuery(Guid CourseId) : IRequest<IEnumerable<EnrollmentDto>>;

public class GetEnrollmentsByCourseQueryHandler : IRequestHandler<GetEnrollmentsByCourseQuery, IEnumerable<EnrollmentDto>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetEnrollmentsByCourseQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> Handle(GetEnrollmentsByCourseQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetByCourseIdAsync(request.CourseId, cancellationToken);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }
}