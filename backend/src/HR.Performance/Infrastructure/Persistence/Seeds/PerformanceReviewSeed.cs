namespace HR.Performance.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Performance.Domain;

/// <summary>
/// Seed data for PerformanceReview entity.
/// </summary>
public static class PerformanceReviewSeed
{
    /// <summary>
    /// Applies seed data for PerformanceReview to the model builder.
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Add seed data if needed
        // Example:
        // modelBuilder.Entity<PerformanceReview>().HasData(
        //     new PerformanceReview { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), ... }
        // );
    }
}
