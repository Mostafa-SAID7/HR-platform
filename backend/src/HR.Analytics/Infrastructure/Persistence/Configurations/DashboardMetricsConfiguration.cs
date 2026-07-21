namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for DashboardMetrics aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class DashboardMetricsConfiguration : IEntityTypeConfiguration<DashboardMetrics>
{
    /// <summary>
    /// Configures the DashboardMetrics entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<DashboardMetrics> builder)
    {
        builder.ToTable("DashboardMetrics");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.ComputedDate);
        
        builder.Property(e => e.AverageBasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.AverageNetSalary).HasPrecision(18, 2);
        builder.Property(e => e.AveragePerformanceRating).HasPrecision(3, 2);
        builder.Property(e => e.AverageAttendancePercentage).HasPrecision(5, 2);
    }
}
