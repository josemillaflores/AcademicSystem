using AutoMapper;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetAllPaymentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    Guid? StudentId = null
) : IRequest<PagedResult<PaymentDto>>;

public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetAllPaymentsQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var paymentList = payments.ToList();
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<PaymentStatus>(request.Status);
            paymentList = paymentList.Where(p => p.Status == status).ToList();
        }
        
        if (request.StudentId.HasValue)
            paymentList = paymentList.Where(p => p.StudentId == request.StudentId.Value).ToList();
        
        var totalCount = paymentList.Count;
        var items = paymentList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
        
        var dtos = _mapper.Map<IEnumerable<PaymentDto>>(items);
        
        return new PagedResult<PaymentDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}