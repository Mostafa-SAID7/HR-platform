namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Domain;
using HR.Payroll.Application.Dtos.Payroll;

/// <summary>
/// Centralized DTO mapping configuration for Payroll aggregate.
/// </summary>
public static class PayrollDtoMappingConfiguration
{
    /// <summary>
    /// Map PayrollRecord domain entity to DTO.
    /// </summary>
    public static PayrollRecordDto ToPayrollDto(this PayrollRecord payroll)
    {
        return payroll.Adapt<PayrollRecordDto>();
    }
}
