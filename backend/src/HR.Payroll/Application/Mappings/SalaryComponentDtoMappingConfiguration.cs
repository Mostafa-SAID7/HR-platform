namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Domain;
using HR.Payroll.Application.Dtos.SalaryComponent;

/// <summary>
/// Centralized DTO mapping configuration for SalaryComponent aggregate.
/// </summary>
public static class SalaryComponentDtoMappingConfiguration
{
    /// <summary>
    /// Map SalaryComponent domain entity to DTO.
    /// </summary>
    public static SalaryComponentDto ToSalaryComponentDto(this SalaryComponent component)
    {
        return component.Adapt<SalaryComponentDto>();
    }
}
