using StudentService.Domain.Interfaces;
using StudentService.Infrastructure.Data;

namespace StudentService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StudentDbContext _context;
    private bool _disposed;

    public UnitOfWork(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesWithDispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        // Obtener entidades con eventos de dominio
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<Student>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Guardar cambios
        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        // Disparar eventos de dominio
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            
            foreach (var domainEvent in events)
            {
                // Aquí se publicarían los eventos de dominio
                // await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}