namespace HR.Audit.Application.Dtos.CompliancePolicy;

public record CreateCompliancePolicyRequest(
    string Name,
    string Description,
    List<string> AuditedEntities,
    List<string> CriticalActions,
    int RetentionDays);
