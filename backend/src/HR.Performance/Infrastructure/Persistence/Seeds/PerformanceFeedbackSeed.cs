namespace HR.Performance.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Performance.Domain;

/// <summary>
/// Seed data for PerformanceFeedback entity.
/// </summary>
public static class PerformanceFeedbackSeed
{
    /// <summary>
    /// Applies seed data for PerformanceFeedback to the model builder.
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Add seed data if needed
        // Example:
        // modelBuilder.Entity<PerformanceFeedback>().HasData(
        //     new PerformanceFeedback { Id = Guid.NewGuid(), FeedbackProviderId = Guid.NewGuid(), ... }
        // );
    }
}
