namespace HR.Attendance.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Attendance.Domain;
using HR.Attendance.Infrastructure.Persistence.Configurations;
using HR.Common.Outbox;

/// <summary>
/// Entity Framework Core database context for Attendance Service.
/// Clean design: Configurations extracted into separate files.
/// </summary>
public class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options) { }

    public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;
    public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
    public DbSet<EmployeeShift> EmployeeShifts { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AttendanceRecordConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveRequestConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeShiftConfiguration());

        // Configure Outbox
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ProcessedOnUtc);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
