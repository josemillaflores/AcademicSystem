using EnrollmentService.Domain.Interfaces;
using EnrollmentService.Infrastructure.Data;

namespace EnrollmentService.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly EnrollmentDbContext _context;

    public UnitOfWork(EnrollmentDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
