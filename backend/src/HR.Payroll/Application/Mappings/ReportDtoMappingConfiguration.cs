namespace HR.Payroll.Application.Mappings;

using Mapster;
using HR.Payroll.Application.Dtos.Report;

/// <summary>
/// Centralized DTO mapping configuration for Report aggregates.
/// </summary>
public static class ReportDtoMappingConfiguration
{
    /// <summary>
    /// Map report data to PayrollReportDto.
    /// </summary>
    public static PayrollReportDto ToPayrollReportDto(this object reportData)
    {
        return reportData.Adapt<PayrollReportDto>();
    }
}
