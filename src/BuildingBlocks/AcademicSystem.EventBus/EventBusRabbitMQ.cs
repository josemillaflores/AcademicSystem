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
    private readonly IModel _channel;
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
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
    }
    
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var eventName = @event.EventType;
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(_exchangeName, eventName, null, body);
        _logger.LogInformation("Event published: {EventName} - {EventId}", eventName, @event.Id);
        
        await Task.CompletedTask;
    }
    
    public Task SubscribeAsync<T, TH>() where T : IIntegrationEvent where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        
        if (!_handlers.ContainsKey(eventName))
            _handlers[eventName] = new List<Type>();
        
        _handlers[eventName].Add(typeof(TH));
        
        _channel.QueueDeclare(eventName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(eventName, _exchangeName, eventName);
        
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await ProcessEvent(eventName, message);
            _channel.BasicAck(args.DeliveryTag, false);
        };
        
        _channel.BasicConsume(eventName, false, consumer);
        
        return Task.CompletedTask;
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
        _channel?.Close();
        _connection?.Close();
    }
}