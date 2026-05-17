using AutoMapper;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentTransactionsQuery(Guid PaymentId) : IRequest<IEnumerable<TransactionDto>>;

public class GetPaymentTransactionsQueryHandler : IRequestHandler<GetPaymentTransactionsQuery, IEnumerable<TransactionDto>>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentTransactionsQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionDto>> Handle(GetPaymentTransactionsQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);
        
        if (payment == null)
            return Enumerable.Empty<TransactionDto>();
            
        return _mapper.Map<IEnumerable<TransactionDto>>(payment.Transactions);
    }
}