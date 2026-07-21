namespace HR.Analytics.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Analytics.Domain;

/// <summary>
/// Seed data for DashboardMetrics entity.
/// Organized separately from DbContext for clean separation of concerns.
/// </summary>
public static class DashboardMetricsSeed
{
    /// <summary>
    /// Seeds initial dashboard metrics data.
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var metricsId = new Guid("11111111-1111-1111-1111-111111111111");
        
        modelBuilder.Entity<DashboardMetrics>().HasData(
            new DashboardMetrics
            {
                Id = metricsId,
                TotalEmployees = 0,
                ActiveEmployees = 0,
                AverageBasicSalary = 0m,
                AveragePerformanceRating = 0m,
                AverageAttendancePercentage = 0m,
                TotalDepartments = 0,
                AverageNetSalary = 0m,
                ComputedDate = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow
            }
        );
    }
}
