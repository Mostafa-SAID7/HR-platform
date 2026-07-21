namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for PerformanceAnalytics aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class PerformanceAnalyticsConfiguration : IEntityTypeConfiguration<PerformanceAnalytics>
{
    /// <summary>
    /// Configures the PerformanceAnalytics entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<PerformanceAnalytics> builder)
    {
        builder.ToTable("PerformanceAnalytics");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Quarter });
        
        builder.Property(e => e.EmployeeName).HasMaxLength(256);
        builder.Property(e => e.AverageRating).HasPrecision(3, 2);
        builder.Property(e => e.Status).HasMaxLength(50);
    }
}
