namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
