namespace HR.Performance.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Performance.Domain;

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

        // Configure PerformanceReview
        modelBuilder.Entity<PerformanceReview>(entity =>
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
        });

        // Configure PerformanceGoal
        modelBuilder.Entity<PerformanceGoal>(entity =>
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
        });

        // Configure PerformanceFeedback
        modelBuilder.Entity<PerformanceFeedback>(entity =>
        {
            entity.ToTable("PerformanceFeedback");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PerformanceReviewId);
            entity.Property(e => e.FeedbackCategory).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Outbox
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ProcessedOnUtc);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
