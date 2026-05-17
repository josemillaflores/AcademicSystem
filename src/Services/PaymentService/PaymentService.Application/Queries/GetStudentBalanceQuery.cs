using AutoMapper;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetStudentBalanceQuery(Guid StudentId) : IRequest<StudentBalanceDto>;

public class GetStudentBalanceQueryHandler : IRequestHandler<GetStudentBalanceQuery, StudentBalanceDto>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public GetStudentBalanceQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<StudentBalanceDto> Handle(GetStudentBalanceQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetByStudentIdAsync(request.StudentId, cancellationToken);
        var paymentList = payments.ToList();
        
        var totalPaid = paymentList
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount.Amount);
        
        var totalPending = paymentList
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .Sum(p => p.Amount.Amount);
        
        var balance = totalPending - totalPaid;
        
        return new StudentBalanceDto(
            StudentId: request.StudentId,
            StudentName: string.Empty,
            StudentNumber: string.Empty,
            TotalPaid: totalPaid,
            TotalPending: totalPending,
            Balance: balance,
            BalanceStatus: balance > 0 ? "Outstanding" : balance < 0 ? "Credit" : "Zero",
            RecentPayments: _mapper.Map<List<PaymentSummaryDto>>(paymentList.Take(5))
        );
    }
}