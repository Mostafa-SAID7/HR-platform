namespace HR.Analytics.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core database context for Analytics Service.
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

        // Configure AnalyticsEvent
        modelBuilder.Entity<AnalyticsEvent>(entity =>
        {
            entity.ToTable("AnalyticsEvents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.EventTimestamp);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
        });

        // Configure EmployeeAnalytics
        modelBuilder.Entity<EmployeeAnalytics>(entity =>
        {
            entity.ToTable("EmployeeAnalytics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeId);
            entity.Property(e => e.EmployeeName).HasMaxLength(256);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configure PayrollAnalytics
        modelBuilder.Entity<PayrollAnalytics>(entity =>
        {
            entity.ToTable("PayrollAnalytics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
            entity.Property(e => e.EmployeeName).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.GrossIncome).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
        });

        // Configure PerformanceAnalytics
        modelBuilder.Entity<PerformanceAnalytics>(entity =>
        {
            entity.ToTable("PerformanceAnalytics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Quarter });
            entity.Property(e => e.EmployeeName).HasMaxLength(256);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // Configure AttendanceAnalytics
        modelBuilder.Entity<AttendanceAnalytics>(entity =>
        {
            entity.ToTable("AttendanceAnalytics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
            entity.Property(e => e.EmployeeName).HasMaxLength(256);
            entity.Property(e => e.AverageWorkHours).HasPrecision(5, 2);
        });

        // Configure DashboardMetrics
        modelBuilder.Entity<DashboardMetrics>(entity =>
        {
            entity.ToTable("DashboardMetrics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ComputedDate);
            entity.Property(e => e.AverageBasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.AverageNetSalary).HasPrecision(18, 2);
            entity.Property(e => e.AveragePerformanceRating).HasPrecision(3, 2);
            entity.Property(e => e.AverageAttendancePercentage).HasPrecision(5, 2);
        });
    }
}
