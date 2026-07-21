namespace HR.Audit.Application.Dtos.AuditEvent;

public record AuditEventDto(
    Guid Id,
    Guid EntityId,
    string EntityType,
    string Action,
    Guid? UserId,
    string? UserEmail,
    DateTime Timestamp,
    string? OldValues,
    string? NewValues,
    string? Reason,
    string Severity,
    string? IpAddress);
