namespace HR.Attendance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Attendance.Domain;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.StartDate, e.TenantId });
        builder.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.LeaveType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.LeaveDays).HasPrecision(5, 2);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
