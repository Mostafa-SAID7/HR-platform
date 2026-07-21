namespace HR.Audit.Domain.AuditTrail;

using HR.Audit.Domain.AuditEvent;

/// <summary>
/// Audit Trail - aggregates multiple audit events for an entity
/// </summary>
public class AuditTrail
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!;
    public List<AuditEvent> Events { get; set; } = [];
    public DateTime FirstChangeAt { get; set; }
    public DateTime LastChangeAt { get; set; }
    public int ChangeCount { get; set; }
    public List<string> AffectedUsers { get; set; } = [];

    private AuditTrail() { }

    /// <summary>
    /// Create a new audit trail
    /// </summary>
    public static AuditTrail Create(Guid entityId, string entityType)
    {
        return new AuditTrail
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = entityType
        };
    }

    /// <summary>
    /// Add an audit event to the trail
    /// </summary>
    public void AddEvent(AuditEvent auditEvent)
    {
        Events.Add(auditEvent);
        LastChangeAt = auditEvent.Timestamp;
        ChangeCount++;

        if (FirstChangeAt == default)
            FirstChangeAt = auditEvent.Timestamp;

        if (auditEvent.UserEmail != null && !AffectedUsers.Contains(auditEvent.UserEmail))
            AffectedUsers.Add(auditEvent.UserEmail);
    }

    /// <summary>
    /// Get change summary for compliance reporting
    /// </summary>
    public AuditTrailSummary GetSummary()
    {
        return new AuditTrailSummary(
            EntityId,
            EntityType,
            FirstChangeAt,
            LastChangeAt,
            ChangeCount,
            AffectedUsers,
            Events.Count(e => e.Severity == AuditEventSeverity.Critical),
            Events.Count(e => e.Severity == AuditEventSeverity.Warning),
            Events.Count(e => e.Severity == AuditEventSeverity.Info));
    }
}
