using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Data;
using System.Linq.Expressions;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    private readonly DbSet<Payment> _dbSet;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Payment>();
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Include(p => p.Transactions)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> AddAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Payment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StudentId == studentId)
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .Include(p => p.Transactions)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaidByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StudentId == studentId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);
    }
}