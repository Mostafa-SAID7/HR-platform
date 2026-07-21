namespace HR.Attendance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Attendance.Domain;

/// <summary>
/// Entity Framework Core configuration for AttendanceRecord aggregate.
/// </summary>
public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.AttendanceDate, e.TenantId }).IsUnique();
        builder.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.CheckInLocation).HasMaxLength(256);
        builder.Property(e => e.CheckOutLocation).HasMaxLength(256);
        builder.Property(e => e.WorkHours).HasPrecision(5, 2);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
