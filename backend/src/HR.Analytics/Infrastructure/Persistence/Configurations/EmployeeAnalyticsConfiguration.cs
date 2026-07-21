namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for EmployeeAnalytics aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class EmployeeAnalyticsConfiguration : IEntityTypeConfiguration<EmployeeAnalytics>
{
    /// <summary>
    /// Configures the EmployeeAnalytics entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<EmployeeAnalytics> builder)
    {
        builder.ToTable("EmployeeAnalytics");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.EmployeeId);
        
        builder.Property(e => e.EmployeeName).HasMaxLength(256);
        builder.Property(e => e.Department).HasMaxLength(100);
        builder.Property(e => e.Designation).HasMaxLength(100);
        builder.Property(e => e.Status).HasMaxLength(50);
    }
}
