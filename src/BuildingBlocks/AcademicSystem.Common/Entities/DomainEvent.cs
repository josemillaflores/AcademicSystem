namespace AcademicSystem.Common.Entities;

/// <summary>
/// Clase base abstracta para todos los eventos de dominio
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string EventType => GetType().Name;

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}