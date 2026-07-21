namespace HR.Performance.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Performance.Domain;
using HR.Performance.Infrastructure.Persistence.Configurations;
using HR.Performance.Infrastructure.Persistence.Seeds;

/// <summary>
/// Entity Framework Core database context for Performance Service.
/// </summary>
public class PerformanceDbContext : DbContext
{
    public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options) : base(options) { }

    public DbSet<PerformanceReview> PerformanceReviews { get; set; } = null!;
    public DbSet<PerformanceGoal> PerformanceGoals { get; set; } = null!;
    public DbSet<PerformanceFeedback> PerformanceFeedback { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new PerformanceReviewConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceGoalConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceFeedbackConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Apply seed data
        PerformanceReviewSeed.Seed(modelBuilder);
        PerformanceGoalSeed.Seed(modelBuilder);
        PerformanceFeedbackSeed.Seed(modelBuilder);
    }
}
