namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Resource, e.Action, e.TenantId }).IsUnique();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Resource).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(256);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
