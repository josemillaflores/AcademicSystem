using CourseService.Domain.Interfaces;
using CourseService.Infrastructure.Data;

namespace CourseService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CourseDbContext _context;
    private bool _disposed;

    public UnitOfWork(CourseDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}