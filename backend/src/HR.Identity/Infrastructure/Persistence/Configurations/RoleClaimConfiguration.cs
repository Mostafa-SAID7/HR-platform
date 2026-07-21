namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("RoleClaims");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ClaimType).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
