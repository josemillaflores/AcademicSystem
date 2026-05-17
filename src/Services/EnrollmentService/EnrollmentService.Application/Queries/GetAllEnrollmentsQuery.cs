using AutoMapper;
using MediatR;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Domain.Enums;
using EnrollmentService.Domain.Interfaces;

namespace EnrollmentService.Application.Queries;

public record GetAllEnrollmentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null
) : IRequest<PagedResult<EnrollmentDto>>;

public class GetAllEnrollmentsQueryHandler : IRequestHandler<GetAllEnrollmentsQuery, PagedResult<EnrollmentDto>>
{
    private readonly IEnrollmentRepository _repository;
    private readonly IMapper _mapper;

    public GetAllEnrollmentsQueryHandler(IEnrollmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<EnrollmentDto>> Handle(GetAllEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _repository.GetAllAsync(cancellationToken);
        var enrollmentList = enrollments.ToList();
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<EnrollmentStatus>(request.Status);
            enrollmentList = enrollmentList.Where(e => e.Status == status).ToList();
        }
        
        var totalCount = enrollmentList.Count;
        var items = enrollmentList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
        
        var dtos = _mapper.Map<IEnumerable<EnrollmentDto>>(items);
        
        return new PagedResult<EnrollmentDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}