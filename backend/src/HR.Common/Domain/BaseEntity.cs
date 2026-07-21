namespace HR.Common.Domain;

/// <summary>
/// Base entity for all domain entities with identity and audit fields.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created the entity.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// User ID who last updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Indicates if the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Timestamp when the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedOnUtc { get; set; }

    /// <summary>
    /// User ID who deleted the entity.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy support.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Add a domain event to the entity.
    /// </summary>
    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove a domain event from the entity.
    /// </summary>
    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clear all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public Guid TenantId { get; set; }
}
