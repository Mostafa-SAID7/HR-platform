namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EntityId, e.EntityType, e.TenantId }).IsUnique();
        builder.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
