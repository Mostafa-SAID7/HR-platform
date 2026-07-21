namespace HR.Analytics.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Analytics.Domain;
using HR.Analytics.Infrastructure.Persistence.Configurations;
using HR.Analytics.Infrastructure.Persistence.Seeds;

/// <summary>
/// Entity Framework Core database context for Analytics Service.
/// Clean design: Configurations and Seeds extracted into separate files.
/// </summary>
public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; } = null!;
    public DbSet<EmployeeAnalytics> EmployeeAnalytics { get; set; } = null!;
    public DbSet<PayrollAnalytics> PayrollAnalytics { get; set; } = null!;
    public DbSet<PerformanceAnalytics> PerformanceAnalytics { get; set; } = null!;
    public DbSet<AttendanceAnalytics> AttendanceAnalytics { get; set; } = null!;
    public DbSet<DashboardMetrics> DashboardMetrics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AnalyticsEventConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeAnalyticsConfiguration());
        modelBuilder.ApplyConfiguration(new PayrollAnalyticsConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceAnalyticsConfiguration());
        modelBuilder.ApplyConfiguration(new AttendanceAnalyticsConfiguration());
        modelBuilder.ApplyConfiguration(new DashboardMetricsConfiguration());

        // Seed data
        DashboardMetricsSeed.Seed(modelBuilder);
    }
}
