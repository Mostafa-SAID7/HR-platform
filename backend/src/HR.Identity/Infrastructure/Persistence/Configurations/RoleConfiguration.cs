namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RoleClaims)
            .WithOne(rc => rc.Role)
            .HasForeignKey(rc => rc.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
