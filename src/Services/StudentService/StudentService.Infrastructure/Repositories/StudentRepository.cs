using Microsoft.EntityFrameworkCore;
using StudentService.Domain.Entities;
using StudentService.Domain.Interfaces;
using StudentService.Infrastructure.Data;
using System.Linq.Expressions;

namespace StudentService.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;
    private readonly DbSet<Student> _dbSet;

    public StudentRepository(StudentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<Student>();
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Enrollments)
            .OrderBy(s => s.Name.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Include(s => s.Enrollments)
            .ToListAsync(cancellationToken);
    }

    public async Task<Student> AddAsync(Student entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Student entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Student entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Student, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.StudentNumber.Value == studentNumber, cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetStudentsByStatusAsync(EnrollmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(s => s.Email.Value == email, cancellationToken);
    }
}