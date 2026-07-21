namespace HR.Payroll.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Payroll.Domain;

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("PayrollRecords");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month, e.TenantId }).IsUnique();
        builder.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.BasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.GrossIncome).HasPrecision(18, 2);
        builder.Property(e => e.NetSalary).HasPrecision(18, 2);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
