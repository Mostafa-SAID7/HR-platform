namespace HR.Analytics.Application.Mappings;

using HR.Analytics.Domain;
using HR.Analytics.Application.Dtos.Dashboard;

/// <summary>
/// Centralized mapping configuration for Dashboard DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class DashboardDtoMappingConfiguration
{
    /// <summary>
    /// Maps DashboardMetrics domain entity to DashboardMetricsDto.
    /// </summary>
    public static DashboardMetricsDto ToDto(this DashboardMetrics metrics)
    {
        return new DashboardMetricsDto
        {
            TotalEmployees = metrics.TotalEmployees,
            ActiveEmployees = metrics.ActiveEmployees,
            AverageBasicSalary = metrics.AverageBasicSalary,
            AveragePerformanceRating = metrics.AveragePerformanceRating,
            AverageAttendancePercentage = metrics.AverageAttendancePercentage,
            TotalDepartments = metrics.TotalDepartments,
            ComputedDate = metrics.ComputedDate
        };
    }
}
