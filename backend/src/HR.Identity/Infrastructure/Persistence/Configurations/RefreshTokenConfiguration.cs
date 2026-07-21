namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Token).IsUnique();
        builder.Property(e => e.Token).IsRequired();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
