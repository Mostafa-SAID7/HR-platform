namespace HR.Payroll.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Payroll.Domain;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("SalaryComponents");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.EffectiveFromDate, e.TenantId });
        builder.Property(e => e.ComponentName).HasMaxLength(100);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
