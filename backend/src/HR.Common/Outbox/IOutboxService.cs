namespace HR.Common.Outbox;

/// <summary>
/// Service for managing outbox messages and publishing events.
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Add an event to the outbox for deferred publishing.
    /// </summary>
    Task AddMessageAsync(
        DomainEvent domainEvent,
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple events to the outbox.
    /// </summary>
    Task AddMessagesAsync(
        IEnumerable<DomainEvent> domainEvents,
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unprocessed outbox messages.
    /// </summary>
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a message as processed.
    /// </summary>
    Task MarkAsProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a message as failed.
    /// </summary>
    Task MarkAsFailedAsync(
        Guid messageId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages that have exceeded retry attempts.
    /// </summary>
    Task<IEnumerable<OutboxMessage>> GetDeadLetterMessagesAsync(
        int maxRetries = 3,
        CancellationToken cancellationToken = default);
}
