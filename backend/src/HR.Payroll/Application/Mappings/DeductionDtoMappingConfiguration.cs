namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Domain.Deduction;
using HR.Payroll.Application.Dtos.Deduction;

/// <summary>
/// Centralized DTO mapping configuration for Deduction aggregate.
/// </summary>
public static class DeductionDtoMappingConfiguration
{
    /// <summary>
    /// Map Deduction domain entity to DTO.
    /// </summary>
    public static DeductionDto ToDeductionDto(this Deduction deduction)
    {
        return deduction.Adapt<DeductionDto>();
    }

    /// <summary>
    /// Map AddDeductionRequest to Deduction domain entity.
    /// </summary>
    public static Deduction ToDeductionDomain(this AddDeductionRequest request, Guid tenantId)
    {
        return Deduction.Create(
            request.PayrollRecordId,
            request.EmployeeId,
            Enum.Parse<DeductionType>(request.DeductionType),
            request.DeductionName,
            request.Amount,
            request.Description,
            tenantId);
    }
}
