namespace HR.Payroll.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Payroll.Domain;

public class DeductionConfiguration : IEntityTypeConfiguration<Deduction>
{
    public void Configure(EntityTypeBuilder<Deduction> builder)
    {
        builder.ToTable("Deductions");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.PayrollRecordId, e.TenantId });
        builder.Property(e => e.DeductionType).HasMaxLength(100);
        builder.Property(e => e.DeductionName).HasMaxLength(256);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
