namespace HR.Audit.Domain.AuditTrail;

/// <summary>
/// Summary data for an audit trail - used for compliance reporting
/// </summary>
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
