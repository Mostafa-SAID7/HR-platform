namespace HR.Recruitment.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Recruitment.Domain.Aggregates;

/// <summary>
/// Entity configuration for InterviewSchedule entity
/// </summary>
public class InterviewScheduleConfiguration : IEntityTypeConfiguration<InterviewSchedule>
{
    public void Configure(EntityTypeBuilder<InterviewSchedule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InterviewType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InterviewerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.InterviewerEmail)
            .IsRequired()
            .HasMaxLength(100);
    }
}
