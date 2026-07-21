namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.CompliancePolicy;

/// <summary>
/// Centralized mapping configuration for CompliancePolicy DTOs.
/// </summary>
public static class CompliancePolicyDtoMappingConfiguration
{
    public static CompliancePolicyDto ToDto(this CompliancePolicy compliancePolicy)
    {
        return new CompliancePolicyDto
        {
            Id = compliancePolicy.Id,
            Name = compliancePolicy.Name,
            Description = compliancePolicy.Description,
            Framework = compliancePolicy.Framework,
            Status = compliancePolicy.Status.ToString(),
            CreatedAt = compliancePolicy.CreatedAt,
            LastUpdatedAt = compliancePolicy.LastUpdatedAt,
            IsActive = compliancePolicy.IsActive
        };
    }
}
