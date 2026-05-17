using MediatR;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Domain.Interfaces;
using AutoMapper;

namespace EnrollmentService.Application.Queries;

public record GetEnrollmentQuery(Guid Id) : IRequest<EnrollmentDto?>;

public class GetEnrollmentQueryHandler : IRequestHandler<GetEnrollmentQuery, EnrollmentDto?>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetEnrollmentQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EnrollmentDto?> Handle(GetEnrollmentQuery request, CancellationToken cancellationToken)
    {
        var enrollment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return enrollment == null ? null : _mapper.Map<EnrollmentDto>(enrollment);
    }
}