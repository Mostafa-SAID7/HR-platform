namespace HR.Analytics.Application.Mappings;

using HR.Analytics.Domain;
using HR.Analytics.Application.Dtos.PayrollAnalytics;

/// <summary>
/// Centralized mapping configuration for PayrollAnalytics DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class PayrollAnalyticsDtoMappingConfiguration
{
    /// <summary>
    /// Maps PayrollAnalytics domain entity to PayrollAnalyticsDto.
    /// </summary>
    public static PayrollAnalyticsDto ToDto(this PayrollAnalytics analytics)
    {
        return new PayrollAnalyticsDto
        {
            EmployeeId = analytics.EmployeeId,
            EmployeeName = analytics.EmployeeName,
            Year = analytics.Year,
            Month = analytics.Month,
            GrossIncome = analytics.GrossIncome,
            NetSalary = analytics.NetSalary,
            Status = analytics.Status
        };
    }
}
