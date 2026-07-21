namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for AttendanceAnalytics aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class AttendanceAnalyticsConfiguration : IEntityTypeConfiguration<AttendanceAnalytics>
{
    /// <summary>
    /// Configures the AttendanceAnalytics entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<AttendanceAnalytics> builder)
    {
        builder.ToTable("AttendanceAnalytics");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
        
        builder.Property(e => e.EmployeeName).HasMaxLength(256);
        builder.Property(e => e.AverageWorkHours).HasPrecision(5, 2);
    }
}
