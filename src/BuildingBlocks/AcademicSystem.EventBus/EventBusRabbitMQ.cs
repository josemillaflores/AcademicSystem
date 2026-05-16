using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Newtonsoft.Json;

namespace AcademicSystem.EventBus;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly string _exchangeName = "academic_exchange";
    
    public EventBusRabbitMQ(IServiceProvider serviceProvider, ILogger<EventBusRabbitMQ> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _handlers = new Dictionary<string, List<Type>>();
        
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        _connection = factory.CreateConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync(new CreateChannelOptions(publisherConfirmationsEnabled: false, publisherConfirmationTrackingEnabled: false), CancellationToken.None).GetAwaiter().GetResult();
        
        _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false, null, false, false, CancellationToken.None).GetAwaiter().GetResult();
    }
    
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var eventName = @event.EventType;
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        await _channel.BasicPublishAsync(_exchangeName, eventName, false, new ReadOnlyMemory<byte>(body), cancellationToken);
        _logger.LogInformation("Event published: {EventName} - {EventId}", eventName, @event.Id);
        
        await Task.CompletedTask;
    }
    
    public async Task SubscribeAsync<T, TH>() where T : IIntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        
        if (!_handlers.ContainsKey(eventName))
            _handlers[eventName] = new List<Type>();
        
        _handlers[eventName].Add(typeof(TH));
        
        _channel.QueueDeclareAsync(eventName, true, false, false, null, false, false, CancellationToken.None).GetAwaiter().GetResult();
        _channel.QueueBindAsync(eventName, _exchangeName, eventName, null, false, CancellationToken.None).GetAwaiter().GetResult();
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessEvent(eventName, message);
            await _channel.BasicAckAsync(args.DeliveryTag, false, CancellationToken.None);
        };
        
        await _channel.BasicConsumeAsync(eventName, false, consumer, CancellationToken.None);
    }
    
    private async Task ProcessEvent(string eventName, string message)
    {
        if (_handlers.TryGetValue(eventName, out var handlers))
        {
            var eventType = Type.GetType($"AcademicSystem.EventBus.{eventName}");
            if (eventType != null)
            {
                var @event = JsonConvert.DeserializeObject(message, eventType);
                
                foreach (var handlerType in handlers)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    var method = handlerType.GetMethod("HandleAsync");
                    
                    if (method != null && @event != null)
                    {
                        await (Task)method.Invoke(handler, new[] { @event, CancellationToken.None })!;
                    }
                }
            }
        }
    }
    
    public Task UnsubscribeAsync<T, TH>() where T : IIntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        if (_handlers.ContainsKey(eventName))
        {
            _handlers[eventName].Remove(typeof(TH));
        }
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _channel?.CloseAsync(CancellationToken.None).GetAwaiter().GetResult();
        _connection?.CloseAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}