namespace HR.Audit.Application.Dtos.AuditTrail;

public record AuditTrailDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers);
