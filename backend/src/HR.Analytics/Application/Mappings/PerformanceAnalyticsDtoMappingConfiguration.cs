namespace HR.Analytics.Application.Mappings;

using HR.Analytics.Domain;
using HR.Analytics.Application.Dtos.PerformanceAnalytics;

/// <summary>
/// Centralized mapping configuration for PerformanceAnalytics DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class PerformanceAnalyticsDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceAnalytics domain entity to PerformanceAnalyticsDto.
    /// </summary>
    public static PerformanceAnalyticsDto ToDto(this PerformanceAnalytics analytics)
    {
        return new PerformanceAnalyticsDto
        {
            EmployeeId = analytics.EmployeeId,
            EmployeeName = analytics.EmployeeName,
            Year = analytics.Year,
            Quarter = analytics.Quarter,
            AverageRating = analytics.AverageRating,
            GoalsCompleted = analytics.GoalsCompleted
        };
    }
}
