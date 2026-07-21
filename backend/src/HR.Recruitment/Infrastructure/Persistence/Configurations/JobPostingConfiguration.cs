namespace HR.Recruitment.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Entity configuration for JobPosting aggregate
/// </summary>
public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RequiredSkills)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.HasMany(x => x.Applications)
            .WithOne()
            .HasForeignKey(x => x.JobPostingId);
    }
}
