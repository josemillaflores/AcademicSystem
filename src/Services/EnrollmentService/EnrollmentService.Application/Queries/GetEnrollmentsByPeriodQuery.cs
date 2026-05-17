using MediatR;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Domain.Interfaces;
using AutoMapper;

namespace EnrollmentService.Application.Queries;

public record GetEnrollmentsByPeriodQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<IEnumerable<EnrollmentDto>>;

public class GetEnrollmentsByPeriodQueryHandler : IRequestHandler<GetEnrollmentsByPeriodQuery, IEnumerable<EnrollmentDto>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetEnrollmentsByPeriodQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> Handle(GetEnrollmentsByPeriodQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetByPeriodAsync(request.StartDate, request.EndDate, cancellationToken);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }
}