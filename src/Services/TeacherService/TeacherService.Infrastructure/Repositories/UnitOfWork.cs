using AcademicSystem.Common;
using Microsoft.EntityFrameworkCore;
using TeacherService.Domain.Interfaces;
using TeacherService.Infrastructure.Data;

namespace TeacherService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TeacherDbContext _context;
    private bool _disposed;

    public UnitOfWork(TeacherDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = _context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
            }
            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Dispose()
    {
        if (!_disposed && _context != null)
        {
            _context.Dispose();
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}