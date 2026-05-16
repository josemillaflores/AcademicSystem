using MediatR;
using EnrollmentService.Application.DTOs;

namespace EnrollmentService.Application.Queries;

public record GetEnrollmentsByStudentQuery(Guid StudentId) : IRequest<IEnumerable<EnrollmentDto>>;

public class GetEnrollmentsByStudentQueryHandler : IRequestHandler<GetEnrollmentsByStudentQuery, IEnumerable<EnrollmentDto>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetEnrollmentsByStudentQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> Handle(GetEnrollmentsByStudentQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
    }
}