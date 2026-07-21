namespace HR.Audit.Domain.AuditEvent;

using System.Text.Json;

/// <summary>
/// Audit Event - immutable record of a change in the system
/// </summary>
public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!; // e.g., "Employee", "Performance", "Payroll"
    public string Action { get; set; } = null!; // e.g., "Created", "Updated", "Deleted", "Approved"
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Reason { get; set; } // Audit trail reason
    public Dictionary<string, string> Metadata { get; set; } = []; // Additional context
    public AuditEventSeverity Severity { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    private AuditEvent() { }

    /// <summary>
    /// Create a new audit event from a domain event
    /// </summary>
    public static AuditEvent FromEvent(
        Guid entityId,
        string entityType,
        string action,
        Guid? userId,
        string? userEmail,
        object? oldValues = null,
        object? newValues = null,
        string? reason = null,
        Dictionary<string, string>? metadata = null,
        AuditEventSeverity severity = AuditEventSeverity.Info,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Reason = reason,
            Metadata = metadata ?? [],
            Severity = severity,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
}
