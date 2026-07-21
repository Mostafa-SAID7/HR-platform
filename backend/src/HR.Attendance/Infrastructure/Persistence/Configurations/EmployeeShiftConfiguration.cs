namespace HR.Attendance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Attendance.Domain;

/// <summary>
/// Entity Framework Core configuration for EmployeeShift aggregate.
/// </summary>
public class EmployeeShiftConfiguration : IEntityTypeConfiguration<EmployeeShift>
{
    public void Configure(EntityTypeBuilder<EmployeeShift> builder)
    {
        builder.ToTable("EmployeeShifts");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.EffectiveDate, e.TenantId });
        builder.Property(e => e.ShiftName).HasMaxLength(100);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
