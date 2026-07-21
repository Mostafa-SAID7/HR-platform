namespace HR.Audit.Application.Dtos.AuditTrail;

public record AuditTrailSummaryDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers,
    int CriticalEventCount,
    int WarningEventCount,
    int InfoEventCount);
