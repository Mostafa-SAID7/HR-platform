namespace HR.Recruitment.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Entity configuration for OfferLetter entity
/// </summary>
public class OfferLetterConfiguration : IEntityTypeConfiguration<OfferLetter>
{
    public void Configure(EntityTypeBuilder<OfferLetter> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);
    }
}
