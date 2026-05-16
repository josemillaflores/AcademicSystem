namespace AcademicSystem.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
    Task SubscribeAsync<T, TH>() where T : IIntegrationEvent where TH : IIntegrationEventHandler<T>;
    Task UnsubscribeAsync<T, TH>() where T : IIntegrationEvent where TH : IIntegrationEventHandler<T>;
}

public interface IIntegrationEventHandler<in T> where T : IIntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}