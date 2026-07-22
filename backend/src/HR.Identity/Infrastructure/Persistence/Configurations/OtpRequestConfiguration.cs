namespace HR.Identity.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Identity.Domain;

/// <summary>
/// Entity Framework configuration for OtpRequest
/// </summary>
public class OtpRequestConfiguration : IEntityTypeConfiguration<OtpRequest>
{
    public void Configure(EntityTypeBuilder<OtpRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.OtpCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.OtpType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        builder.Property(x => x.AttemptCount)
            .HasDefaultValue(0);

        builder.Property(x => x.MaxAttempts)
            .HasDefaultValue(3);

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.Property(x => x.ExpiryTime)
            .IsRequired();

        builder.Property(x => x.UsedAt);

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.PhoneNumber);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.Email, x.OtpType });
        builder.HasIndex(x => x.ExpiryTime);
    }
}
