using Microsoft.EntityFrameworkCore;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Domain.Enums;
using EnrollmentService.Domain.Interfaces;
using EnrollmentService.Infrastructure.Data;
using System.Linq.Expressions;

namespace EnrollmentService.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly EnrollmentDbContext _context;
    private readonly DbSet<Enrollment> _dbSet;

    public EnrollmentRepository(EnrollmentDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Enrollment>();
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Validations)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Validations)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> FindAsync(Expression<Func<Enrollment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Include(e => e.Validations)
            .ToListAsync(cancellationToken);
    }

    public async Task<Enrollment> AddAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Enrollment entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Enrollment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Validations)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Validations)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByStatusAsync(EnrollmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == status)
            .Include(e => e.Validations)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.EnrollmentDate >= startDate && e.EnrollmentDate <= endDate)
            .Include(e => e.Validations)
            .ToListAsync(cancellationToken);
    }
}