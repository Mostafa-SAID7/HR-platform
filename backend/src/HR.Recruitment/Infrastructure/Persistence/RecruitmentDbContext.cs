namespace HR.Recruitment.Infrastructure.Persistence;

using HR.Recruitment.Infrastructure.Persistence.Configurations;
using HR.Recruitment.Infrastructure.Persistence.Seeds;

/// <summary>
/// Database context for Recruitment Service
/// Applies entity configurations from separate configuration classes following SOLID principles
/// Seeds initial data from dedicated seed classes
/// </summary>
public class RecruitmentDbContext : DbContext, IUnitOfWork
{
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
    public DbSet<OfferLetter> OfferLetters { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options) { }

    /// <summary>
    /// Configure entity mappings by applying separate configuration classes
    /// Each entity type has its own focused configuration following Single Responsibility Principle
    /// Seed data applied from dedicated seed classes organized by aggregate
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from separate classes
        modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
        modelBuilder.ApplyConfiguration(new JobApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new InterviewScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new OfferLetterConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Apply seed data from dedicated seed classes
        JobPostingSeed.Seed(modelBuilder);
        JobApplicationSeed.Seed(modelBuilder);
        InterviewScheduleSeed.Seed(modelBuilder);
        OfferLetterSeed.Seed(modelBuilder);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Save domain events to outbox before committing
        var domainEventEntities = ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(x => x.Entity.GetDomainEvents())
            .ToList();

        if (domainEventEntities.Count > 0)
        {
            var outboxService = new OutboxService();
            foreach (var domainEvent in domainEventEntities)
            {
                var outboxMessage = outboxService.CreateOutboxMessage(domainEvent);
                await OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            }
        }

        var result = await SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
