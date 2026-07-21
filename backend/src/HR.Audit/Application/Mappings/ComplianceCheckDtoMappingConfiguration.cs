namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.ComplianceCheck;

/// <summary>
/// Centralized mapping configuration for ComplianceCheck DTOs.
/// </summary>
public static class ComplianceCheckDtoMappingConfiguration
{
    public static ComplianceCheckDto ToDto(this ComplianceCheck complianceCheck)
    {
        return new ComplianceCheckDto
        {
            Id = complianceCheck.Id,
            PolicyId = complianceCheck.PolicyId,
            EntityId = complianceCheck.EntityId,
            EntityType = complianceCheck.EntityType,
            Status = complianceCheck.Status.ToString(),
            CheckedAt = complianceCheck.CheckedAt,
            Result = complianceCheck.Result,
            Remarks = complianceCheck.Remarks
        };
    }
}
