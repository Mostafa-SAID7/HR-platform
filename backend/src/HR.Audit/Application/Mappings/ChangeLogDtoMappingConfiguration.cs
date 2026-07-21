namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.ChangeLog;

/// <summary>
/// Centralized mapping configuration for ChangeLog DTOs.
/// </summary>
public static class ChangeLogDtoMappingConfiguration
{
    public static ChangeLogDto ToDto(this ChangeLog changeLog)
    {
        return new ChangeLogDto
        {
            Id = changeLog.Id,
            EntityId = changeLog.EntityId,
            EntityType = changeLog.EntityType,
            Action = changeLog.Action,
            ChangedBy = changeLog.ChangedBy,
            ChangedAt = changeLog.ChangedAt,
            OldValue = changeLog.OldValue,
            NewValue = changeLog.NewValue,
            FieldName = changeLog.FieldName
        };
    }
}
