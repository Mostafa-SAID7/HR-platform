namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
        builder.HasIndex(e => new { e.Username, e.TenantId }).IsUnique();
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Username).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.UserClaims)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
