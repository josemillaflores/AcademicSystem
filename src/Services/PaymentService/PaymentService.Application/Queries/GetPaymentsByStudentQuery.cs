using AutoMapper;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentsByStudentQuery(Guid StudentId) : IRequest<IEnumerable<PaymentDto>>;

public class GetPaymentsByStudentQueryHandler : IRequestHandler<GetPaymentsByStudentQuery, IEnumerable<PaymentDto>>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetPaymentsByStudentQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByStudentQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }
}