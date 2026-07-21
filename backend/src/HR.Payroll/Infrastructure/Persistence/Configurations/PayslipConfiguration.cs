namespace HR.Payroll.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Payroll.Domain;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
        builder.Property(e => e.PayslipContent).HasColumnType("text");
    }
}
