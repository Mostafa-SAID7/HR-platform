namespace HR.Common.Outbox;

/// <summary>
/// Outbox message for guaranteed event delivery using the Outbox pattern.
/// </summary>
public class OutboxMessage : BaseEntity
{
    /// <summary>
    /// Type of the event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Serialized event data (JSON).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Timestamp when the event was published to message broker.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Indicates if the event has been published.
    /// </summary>
    public bool IsProcessed => ProcessedOnUtc.HasValue;

    /// <summary>
    /// Number of retry attempts.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Service/topic name for routing.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
}
