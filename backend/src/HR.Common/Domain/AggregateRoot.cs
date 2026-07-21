namespace HR.Common.Domain;

/// <summary>
/// Base class for aggregate roots with domain event tracking.
/// Aggregates are clusters of associated objects treated as a unit with respect to data changes.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    /// <summary>
    /// Version for optimistic concurrency control.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Increment version for concurrency control.
    /// </summary>
    protected void IncrementVersion()
    {
        Version++;
    }

    /// <summary>
    /// Clear all domain events after they have been published.
    /// </summary>
    public void MarkEventsAsPublished()
    {
        ClearDomainEvents();
    }
}
