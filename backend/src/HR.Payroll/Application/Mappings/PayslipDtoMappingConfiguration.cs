namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Domain;
using HR.Payroll.Application.Dtos.Payslip;

/// <summary>
/// Centralized DTO mapping configuration for Payslip aggregate.
/// </summary>
public static class PayslipDtoMappingConfiguration
{
    /// <summary>
    /// Map Payslip domain entity to DTO.
    /// </summary>
    public static PayslipDto ToPayslipDto(this Payslip payslip)
    {
        return payslip.Adapt<PayslipDto>();
    }
}
