using AcademicSystem.Common.Entities;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaidByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}