namespace HR.Performance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Performance.Domain;

/// <summary>
/// Entity Framework Core configuration for PerformanceReview.
/// </summary>
public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> entity)
    {
        entity.ToTable("PerformanceReviews");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.EmployeeId, e.ReviewYear, e.ReviewQuarter, e.TenantId }).IsUnique();
        entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        entity.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
        entity.Property(e => e.ReviewerName).HasMaxLength(256).IsRequired();
        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasMany(e => e.Goals)
            .WithOne(g => g.PerformanceReview)
            .HasForeignKey(g => g.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Feedback)
            .WithOne(f => f.PerformanceReview)
            .HasForeignKey(f => f.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
