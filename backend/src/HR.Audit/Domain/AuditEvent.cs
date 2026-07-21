namespace HR.Audit.Domain;

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
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
            Reason = reason,
            Metadata = metadata ?? [],
            Severity = severity,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
}

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

/// <summary>
/// Compliance Policy - rules for what should be audited
/// </summary>
public class CompliancePolicy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<string> AuditedEntities { get; set; } = []; // e.g., ["Employee", "Payroll", "Performance"]
    public List<string> CriticalActions { get; set; } = []; // e.g., ["Delete", "Approve", "Process"]
    public int RetentionDays { get; set; } = 2555; // ~7 years for compliance
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool ShouldAudit(string entityType, string action)
    {
        return AuditedEntities.Contains(entityType) || CriticalActions.Contains(action);
    }

    public bool IsCriticalAction(string action)
    {
        return CriticalActions.Contains(action);
    }
}

/// <summary>
/// Audit Report - for compliance and investigation
/// </summary>
public class AuditReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportType Type { get; set; }
    public ReportStatus Status { get; set; }
    public List<AuditEvent> Events { get; set; } = [];
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public Guid? GeneratedByUserId { get; set; }

    public static AuditReport CreateComplianceReport(
        string title,
        DateTime startDate,
        DateTime endDate,
        Guid generatedByUserId)
    {
        return new AuditReport
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = $"Compliance audit report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            GeneratedAt = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Type = ReportType.Compliance,
            Status = ReportStatus.Generated,
            GeneratedByUserId = generatedByUserId
        };
    }

    public static AuditReport CreateIncidentReport(
        string title,
        string description,
        Guid generatedByUserId)
    {
        return new AuditReport
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            GeneratedAt = DateTime.UtcNow,
            Type = ReportType.Incident,
            Status = ReportStatus.InProgress,
            GeneratedByUserId = generatedByUserId
        };
    }
}

// DTOs and supporting types
public record AuditTrailSummary(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers,
    int CriticalEventCount,
    int WarningEventCount,
    int InfoEventCount);

public enum AuditEventSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

public enum ReportType
{
    Compliance = 0,
    Incident = 1,
    Investigation = 2,
    ChangeLog = 3
}

public enum ReportStatus
{
    Draft = 0,
    InProgress = 1,
    Generated = 2,
    Published = 3,
    Archived = 4
}
