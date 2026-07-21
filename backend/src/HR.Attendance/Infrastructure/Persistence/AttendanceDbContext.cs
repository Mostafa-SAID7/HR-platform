namespace HR.Attendance.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Attendance.Domain;

/// <summary>
/// Entity Framework Core database context for Attendance Service.
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

        // Configure AttendanceRecord
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("AttendanceRecords");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate, e.TenantId }).IsUnique();
            entity.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.CheckInLocation).HasMaxLength(256);
            entity.Property(e => e.CheckOutLocation).HasMaxLength(256);
            entity.Property(e => e.WorkHours).HasPrecision(5, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure LeaveRequest
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("LeaveRequests");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.StartDate, e.TenantId });
            entity.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.LeaveType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.LeaveDays).HasPrecision(5, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure EmployeeShift
        modelBuilder.Entity<EmployeeShift>(entity =>
        {
            entity.ToTable("EmployeeShifts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveDate, e.TenantId });
            entity.Property(e => e.ShiftName).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

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
