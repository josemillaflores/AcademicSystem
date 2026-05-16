using Microsoft.EntityFrameworkCore;
using TeacherService.Domain.Entities;
using TeacherService.Domain.Interfaces;
using TeacherService.Infrastructure.Data;
using System.Linq.Expressions;

namespace TeacherService.Infrastructure.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly TeacherDbContext _context;
    private readonly DbSet<Teacher> _dbSet;

    public TeacherRepository(TeacherDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Teacher>();
    }

    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Specialties)
            .Include(t => t.CourseAssignments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Specialties)
            .Include(t => t.CourseAssignments)
            .OrderBy(t => t.Name.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> FindAsync(Expression<Func<Teacher, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Include(t => t.Specialties)
            .Include(t => t.CourseAssignments)
            .ToListAsync(cancellationToken);
    }

    public async Task<Teacher> AddAsync(Teacher entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Teacher entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Teacher entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Teacher, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Teacher?> GetByTeacherNumberAsync(string teacherNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.TeacherNumber.Value == teacherNumber, cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Status == TeacherStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Teacher>> GetTeachersBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Specialties.Any(s => s.Id == specialtyId))
            .ToListAsync(cancellationToken);
    }
}