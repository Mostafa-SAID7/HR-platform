namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for PayrollAnalytics aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class PayrollAnalyticsConfiguration : IEntityTypeConfiguration<PayrollAnalytics>
{
    /// <summary>
    /// Configures the PayrollAnalytics entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<PayrollAnalytics> builder)
    {
        builder.ToTable("PayrollAnalytics");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
        
        builder.Property(e => e.EmployeeName).HasMaxLength(256);
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.BasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.GrossIncome).HasPrecision(18, 2);
        builder.Property(e => e.NetSalary).HasPrecision(18, 2);
    }
}
