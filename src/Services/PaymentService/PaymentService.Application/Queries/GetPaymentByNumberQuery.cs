using MediatR;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries;

public record GetPaymentByNumberQuery(string PaymentNumber) : IRequest<PaymentDto?>;

public class GetPaymentByNumberQueryHandler : IRequestHandler<GetPaymentByNumberQuery, PaymentDto?>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentByNumberQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByNumberQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByPaymentNumberAsync(request.PaymentNumber, cancellationToken);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }
}