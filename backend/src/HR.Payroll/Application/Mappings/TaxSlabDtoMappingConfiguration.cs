namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Domain;
using HR.Payroll.Application.Dtos;

/// <summary>
/// Centralized DTO mapping configuration for TaxSlab aggregate.
/// </summary>
public static class TaxSlabDtoMappingConfiguration
{
    /// <summary>
    /// Map TaxSlab domain entity to DTO.
    /// </summary>
    public static TaxSlabDto ToTaxSlabDto(this TaxSlab taxSlab)
    {
        return taxSlab.Adapt<TaxSlabDto>();
    }
}
