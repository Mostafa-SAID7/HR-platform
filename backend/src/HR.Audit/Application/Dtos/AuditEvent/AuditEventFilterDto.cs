namespace HR.Audit.Application.Dtos.AuditEvent;

public record AuditEventFilterDto(
    Guid? EntityId = null,
    string? EntityType = null,
    string? Action = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Severity = null,
    int Page = 1,
    int PageSize = 10);
