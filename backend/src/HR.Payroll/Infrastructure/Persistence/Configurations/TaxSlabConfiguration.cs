namespace HR.Payroll.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Payroll.Domain;

public class TaxSlabConfiguration : IEntityTypeConfiguration<TaxSlab>
{
    public void Configure(EntityTypeBuilder<TaxSlab> builder)
    {
        builder.ToTable("TaxSlabs");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Year, e.MinSalary });
        builder.Property(e => e.MinSalary).HasPrecision(18, 2);
        builder.Property(e => e.MaxSalary).HasPrecision(18, 2);
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
    }
}
