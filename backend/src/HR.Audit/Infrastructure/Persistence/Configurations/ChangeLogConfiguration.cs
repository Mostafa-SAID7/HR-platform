namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
{
    public void Configure(EntityTypeBuilder<ChangeLog> builder)
    {
        builder.ToTable("ChangeLogs");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EntityId, e.EntityType, e.TenantId });
        builder.HasIndex(e => e.ChangedAt);
        builder.Property(e => e.EntityType).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FieldName).HasMaxLength(256);
        builder.Property(e => e.OldValue).HasMaxLength(2000);
        builder.Property(e => e.NewValue).HasMaxLength(2000);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
