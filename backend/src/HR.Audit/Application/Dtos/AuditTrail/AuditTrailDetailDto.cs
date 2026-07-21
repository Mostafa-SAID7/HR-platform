namespace HR.Audit.Application.Dtos.AuditTrail;

using HR.Audit.Application.Dtos.AuditEvent;

public record AuditTrailDetailDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers,
    List<AuditEventDto> Events);
