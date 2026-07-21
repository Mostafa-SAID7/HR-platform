namespace HR.Audit.Application.Dtos.CompliancePolicy;

public record CompliancePolicyDto(
    Guid Id,
    string Name,
    string Description,
    List<string> AuditedEntities,
    List<string> CriticalActions,
    int RetentionDays,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
