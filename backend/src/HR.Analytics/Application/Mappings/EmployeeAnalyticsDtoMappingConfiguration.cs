namespace HR.Analytics.Application.Mappings;

using HR.Analytics.Domain;
using HR.Analytics.Application.Dtos.EmployeeAnalytics;

/// <summary>
/// Centralized mapping configuration for EmployeeAnalytics DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class EmployeeAnalyticsDtoMappingConfiguration
{
    /// <summary>
    /// Maps EmployeeAnalytics domain entity to EmployeeAnalyticsDto.
    /// </summary>
    public static EmployeeAnalyticsDto ToDto(this EmployeeAnalytics analytics)
    {
        return new EmployeeAnalyticsDto
        {
            EmployeeId = analytics.EmployeeId,
            EmployeeName = analytics.EmployeeName,
            Department = analytics.Department,
            Designation = analytics.Designation,
            JoinDate = analytics.JoinDate,
            Status = analytics.Status,
            YearsOfService = analytics.YearsOfService,
            SkillCount = analytics.SkillCount
        };
    }
}
