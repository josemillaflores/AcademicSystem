using AutoMapper;
using MediatR;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Queries;

public record ExportPaymentsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<byte[]>;

public class ExportPaymentsQueryHandler : IRequestHandler<ExportPaymentsQuery, byte[]>
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;

    public ExportPaymentsQueryHandler(IPaymentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<byte[]> Handle(ExportPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var paymentList = payments.ToList();
        
        if (request.StartDate.HasValue)
            paymentList = paymentList.Where(p => p.CreatedAt >= request.StartDate.Value).ToList();
        
        if (request.EndDate.HasValue)
            paymentList = paymentList.Where(p => p.CreatedAt <= request.EndDate.Value).ToList();
        
        // Crear archivo CSV simplificado
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("PaymentId,PaymentNumber,StudentId,Amount,Currency,PaymentDate,Method,Status");
        
        foreach (var payment in paymentList)
        {
            csv.AppendLine($"{payment.Id},{payment.PaymentNumber},{payment.StudentId},{payment.Amount.Amount},{payment.Amount.Currency},{payment.PaymentDate:yyyy-MM-dd},{payment.Method},{payment.Status}");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }
}