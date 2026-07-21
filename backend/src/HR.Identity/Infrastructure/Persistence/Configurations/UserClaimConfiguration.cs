namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaims");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ClaimType).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
