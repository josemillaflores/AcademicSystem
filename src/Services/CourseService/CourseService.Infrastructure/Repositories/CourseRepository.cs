using Microsoft.EntityFrameworkCore;
using CourseService.Domain.Entities;
using CourseService.Domain.Interfaces;
using CourseService.Infrastructure.Data;
using System.Linq.Expressions;

namespace CourseService.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly CourseDbContext _context;
    private readonly DbSet<Course> _dbSet;

    public CourseRepository(CourseDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Course>();
    }

    public async Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Prerequisites)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> FindAsync(Expression<Func<Course, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Include(c => c.Prerequisites)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course> AddAsync(Course entity, CancellationToken cancellationToken = default)
    {
        var entry = await _dbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Course entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Course entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Course, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Course?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetAvailableCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == CourseStatus.Active && c.HasAvailableSlots())
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetCoursesByCreditsRangeAsync(int minCredits, int maxCredits, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Credits >= minCredits && c.Credits <= maxCredits)
            .ToListAsync(cancellationToken);
    }
}