using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentMethodSummaryQuery() : IRequest<IEnumerable<PaymentMethodSummaryDto>>;

public class GetPaymentMethodSummaryQueryHandler : IRequestHandler<GetPaymentMethodSummaryQuery, IEnumerable<PaymentMethodSummaryDto>>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentMethodSummaryQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentMethodSummaryDto>> Handle(GetPaymentMethodSummaryQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        
        var totalRevenue = completedPayments.Sum(p => p.Amount.Amount);
        
        var summary = completedPayments
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodSummaryDto(
                PaymentMethod: g.Key.ToString(),
                TotalPayments: g.Count(),
                TotalAmount: g.Sum(p => p.Amount.Amount),
                AverageAmount: g.Average(p => p.Amount.Amount),
                PercentageOfTotal: totalRevenue > 0 ? (double)g.Sum(p => p.Amount.Amount) / (double)totalRevenue * 100 : 0
            ));
        
        return summary;
    }
}