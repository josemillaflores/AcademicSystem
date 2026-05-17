using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentStatisticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<PaymentStatisticsDto>;

public class GetPaymentStatisticsQueryHandler : IRequestHandler<GetPaymentStatisticsQuery, PaymentStatisticsDto>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentStatisticsQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentStatisticsDto> Handle(GetPaymentStatisticsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var paymentList = payments.ToList();
        
        if (request.StartDate.HasValue)
            paymentList = paymentList.Where(p => p.CreatedAt >= request.StartDate.Value).ToList();
        
        if (request.EndDate.HasValue)
            paymentList = paymentList.Where(p => p.CreatedAt <= request.EndDate.Value).ToList();
        
        var completedPayments = paymentList.Count(p => p.Status == PaymentStatus.Completed);
        var pendingPayments = paymentList.Count(p => p.Status == PaymentStatus.Pending);
        var failedPayments = paymentList.Count(p => p.Status == PaymentStatus.Failed);
        
        var totalRevenue = paymentList
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount.Amount);
        
        var revenueThisMonth = paymentList
            .Where(p => p.Status == PaymentStatus.Completed && p.CreatedAt.Month == DateTime.UtcNow.Month)
            .Sum(p => p.Amount.Amount);
        
        var successRate = paymentList.Count > 0 
            ? (double)completedPayments / paymentList.Count * 100 
            : 0;
        
        var revenueByMethod = paymentList
            .Where(p => p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.Method.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount.Amount));
        
        return new PaymentStatisticsDto(
            TotalPayments: paymentList.Count,
            CompletedPayments: completedPayments,
            PendingPayments: pendingPayments,
            FailedPayments: failedPayments,
            TotalRevenue: totalRevenue,
            AveragePaymentAmount: paymentList.Any() ? paymentList.Average(p => p.Amount.Amount) : 0,
            RevenueThisMonth: revenueThisMonth,
            SuccessRate: successRate,
            RevenueByMethod: revenueByMethod,
            PaymentsByStatus: new Dictionary<string, int>
            {
                ["Completed"] = completedPayments,
                ["Pending"] = pendingPayments,
                ["Failed"] = failedPayments
            }
        );
    }
}