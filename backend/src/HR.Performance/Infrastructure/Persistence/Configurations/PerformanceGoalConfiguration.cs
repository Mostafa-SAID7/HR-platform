namespace HR.Performance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Performance.Domain;

/// <summary>
/// Entity Framework Core configuration for PerformanceGoal.
/// </summary>
public class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> entity)
    {
        entity.ToTable("PerformanceGoals");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.EmployeeId, e.PerformanceReviewId }).IsUnique();
        entity.Property(e => e.GoalTitle).IsRequired().HasMaxLength(256);
        entity.Property(e => e.Status).HasMaxLength(50);
        entity.Property(e => e.UnitOfMeasure).HasMaxLength(50);
        entity.Property(e => e.TargetValue).HasPrecision(19, 2);
        entity.Property(e => e.ActualValue).HasPrecision(19, 2);
        entity.Property(e => e.Weight).HasPrecision(5, 2);
        entity.HasQueryFilter(e => !e.IsDeleted);
    }
}
