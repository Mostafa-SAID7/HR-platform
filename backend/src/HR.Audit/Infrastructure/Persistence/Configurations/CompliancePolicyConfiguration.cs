namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class CompliancePolicyConfiguration : IEntityTypeConfiguration<CompliancePolicy>
{
    public void Configure(EntityTypeBuilder<CompliancePolicy> builder)
    {
        builder.ToTable("CompliancePolicies");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Framework, e.TenantId });
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Framework).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
