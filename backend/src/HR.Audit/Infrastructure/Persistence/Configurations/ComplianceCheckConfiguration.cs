namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class ComplianceCheckConfiguration : IEntityTypeConfiguration<ComplianceCheck>
{
    public void Configure(EntityTypeBuilder<ComplianceCheck> builder)
    {
        builder.ToTable("ComplianceChecks");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.PolicyId, e.CheckedAt });
        builder.HasIndex(e => new { e.EntityId, e.EntityType });
        builder.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.Property(e => e.Result).HasMaxLength(50);
        builder.Property(e => e.Remarks).HasMaxLength(2000);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
