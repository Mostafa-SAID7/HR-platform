namespace HR.Audit.Application.Dtos.AuditEvent;

public record AuditEventDetailDto(
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
    Dictionary<string, string> Metadata,
    string Severity,
    string? IpAddress,
    string? UserAgent);
