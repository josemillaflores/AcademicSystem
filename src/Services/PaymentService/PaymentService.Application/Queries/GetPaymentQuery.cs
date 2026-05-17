using AutoMapper;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentQuery(Guid Id) : IRequest<PaymentDto?>;

public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }
}