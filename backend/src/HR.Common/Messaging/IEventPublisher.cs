namespace HR.Common.Messaging;

/// <summary>
/// Event publisher interface for publishing domain events to message broker.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish a domain event to the message broker.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Publish multiple domain events to the message broker.
    /// </summary>
    Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>
    /// Subscribe to a domain event type.
    /// </summary>
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : DomainEvent;
}

/// <summary>
/// Event handler interface for handling published events.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
