namespace HR.Audit.Features.GetAuditTrail;

/// <summary>
/// Get audit trail for an entity
/// </summary>
public record GetAuditTrailQuery(
    Guid EntityId,
    string EntityType,
    Guid TenantId) : IQuery<AuditTrailDetailDto>;
