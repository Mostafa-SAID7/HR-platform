namespace HR.Performance.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Performance.Domain;

/// <summary>
/// Seed data for PerformanceGoal entity.
/// </summary>
public static class PerformanceGoalSeed
{
    /// <summary>
    /// Applies seed data for PerformanceGoal to the model builder.
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Add seed data if needed
        // Example:
        // modelBuilder.Entity<PerformanceGoal>().HasData(
        //     new PerformanceGoal { Id = Guid.NewGuid(), GoalTitle = "...", ... }
        // );
    }
}
