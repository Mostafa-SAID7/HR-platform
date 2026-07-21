namespace HR.Common.Messaging;

/// <summary>
/// Implementation of IEventPublisher using MassTransit for Kafka integration.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<EventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        try
        {
            _logger.LogInformation("Publishing event {EventType}", typeof(TEvent).Name);
            
            await _publishEndpoint.Publish(@event, cancellationToken);
            
            _logger.LogInformation("Event {EventType} published successfully", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    public async Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        try
        {
            _logger.LogInformation("Publishing {Count} events of type {EventType}", events.Count(), typeof(TEvent).Name);
            
            foreach (var @event in events)
            {
                await _publishEndpoint.Publish(@event, cancellationToken);
            }
            
            _logger.LogInformation("Events of type {EventType} published successfully", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish events of type {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : DomainEvent
    {
        // This is handled by MassTransit's consumer registration
        // Not needed for publish-subscribe pattern with MassTransit
        _logger.LogInformation("Handler registered for event type {EventType}", typeof(TEvent).Name);
    }
}
