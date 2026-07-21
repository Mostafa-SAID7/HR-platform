namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EntityId, e.EntityType, e.TenantId });
        builder.HasIndex(e => e.Timestamp);
        builder.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(100).IsRequired();
        builder.Property(e => e.UserEmail).HasMaxLength(256);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
