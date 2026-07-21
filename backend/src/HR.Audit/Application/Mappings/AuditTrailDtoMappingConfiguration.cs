namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.AuditTrail;
using HR.Audit.Application.Dtos.AuditEvent;

/// <summary>
/// Centralized mapping configuration for AuditTrail DTOs.
/// </summary>
public static class AuditTrailDtoMappingConfiguration
{
    public static AuditTrailDto ToDto(this AuditTrail auditTrail)
    {
        return new AuditTrailDto
        {
            EntityId = auditTrail.EntityId,
            EntityType = auditTrail.EntityType,
            FirstChangeAt = auditTrail.FirstChangeAt,
            LastChangeAt = auditTrail.LastChangeAt,
            ChangeCount = auditTrail.ChangeCount
        };
    }

    public static AuditTrailDetailDto ToDetailDto(this AuditTrail auditTrail)
    {
        return new AuditTrailDetailDto(
            auditTrail.EntityId,
            auditTrail.EntityType,
            auditTrail.FirstChangeAt,
            auditTrail.LastChangeAt,
            auditTrail.ChangeCount,
            auditTrail.AffectedUsers,
            auditTrail.Events.Select(e => e.ToDto()).ToList());
    }

    public static AuditTrailSummaryDto ToSummaryDto(this AuditTrail auditTrail)
    {
        return new AuditTrailSummaryDto
        {
            EntityId = auditTrail.EntityId,
            EntityType = auditTrail.EntityType,
            LastModifiedAt = auditTrail.LastChangeAt,
            ModificationCount = auditTrail.ChangeCount,
            LastModifiedBy = auditTrail.AffectedUsers.LastOrDefault()
        };
    }
}
