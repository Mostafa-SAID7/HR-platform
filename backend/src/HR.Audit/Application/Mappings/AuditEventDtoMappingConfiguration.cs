namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.AuditEvent;

/// <summary>
/// Centralized mapping configuration for AuditEvent DTOs.
/// </summary>
public static class AuditEventDtoMappingConfiguration
{
    public static AuditEventDto ToDto(this AuditEvent auditEvent)
    {
        return new AuditEventDto(
            auditEvent.Id,
            auditEvent.EntityId,
            auditEvent.EntityType,
            auditEvent.Action,
            auditEvent.UserId,
            auditEvent.UserEmail,
            auditEvent.Timestamp,
            auditEvent.OldValues,
            auditEvent.NewValues,
            auditEvent.Reason,
            auditEvent.Severity.ToString(),
            auditEvent.IpAddress);
    }

    public static AuditEventDetailDto ToDetailDto(this AuditEvent auditEvent)
    {
        return new AuditEventDetailDto
        {
            Id = auditEvent.Id,
            EntityId = auditEvent.EntityId,
            EntityType = auditEvent.EntityType,
            Action = auditEvent.Action,
            UserId = auditEvent.UserId,
            UserEmail = auditEvent.UserEmail,
            Timestamp = auditEvent.Timestamp,
            OldValues = auditEvent.OldValues,
            NewValues = auditEvent.NewValues,
            Reason = auditEvent.Reason,
            Severity = auditEvent.Severity.ToString(),
            IpAddress = auditEvent.IpAddress
        };
    }
}
