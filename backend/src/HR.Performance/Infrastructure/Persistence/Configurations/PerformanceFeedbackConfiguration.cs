namespace HR.Performance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Performance.Domain;

/// <summary>
/// Entity Framework Core configuration for PerformanceFeedback.
/// </summary>
public class PerformanceFeedbackConfiguration : IEntityTypeConfiguration<PerformanceFeedback>
{
    public void Configure(EntityTypeBuilder<PerformanceFeedback> entity)
    {
        entity.ToTable("PerformanceFeedback");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.PerformanceReviewId);
        entity.Property(e => e.FeedbackCategory).HasMaxLength(100);
        entity.HasQueryFilter(e => !e.IsDeleted);
    }
}
