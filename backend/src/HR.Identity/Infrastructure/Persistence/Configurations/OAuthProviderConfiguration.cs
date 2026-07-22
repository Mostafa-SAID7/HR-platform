namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

/// <summary>
/// Entity Framework configuration for OAuthProvider
/// </summary>
public class OAuthProviderConfiguration : IEntityTypeConfiguration<OAuthProvider>
{
    public void Configure(EntityTypeBuilder<OAuthProvider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ProviderType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ProviderEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ProviderName)
            .HasMaxLength(50);

        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.AccessToken)
            .HasMaxLength(4096);

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(4096);

        builder.Property(x => x.TokenExpiry);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.ConnectedAt)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ProviderUserId);
        builder.HasIndex(x => x.ProviderEmail);
        builder.HasIndex(x => new { x.UserId, x.ProviderType }).IsUnique();
        builder.HasIndex(x => new { x.ProviderType, x.ProviderUserId }).IsUnique();
    }
}
