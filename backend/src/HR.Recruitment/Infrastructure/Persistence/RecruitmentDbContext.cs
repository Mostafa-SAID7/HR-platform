namespace HR.Recruitment.Infrastructure.Persistence;

/// <summary>
/// Database context for Recruitment Service
/// </summary>
public class RecruitmentDbContext : DbContext, IUnitOfWork
{
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
    public DbSet<OfferLetter> OfferLetters { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // JobPosting configuration
        modelBuilder.Entity<JobPosting>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<JobPosting>()
            .Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<JobPosting>()
            .Property(x => x.Description)
            .IsRequired();

        modelBuilder.Entity<JobPosting>()
            .Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<JobPosting>()
            .Property(x => x.RequiredSkills)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        modelBuilder.Entity<JobPosting>()
            .HasMany(x => x.Applications)
            .WithOne()
            .HasForeignKey(x => x.JobPostingId);

        // JobApplication configuration
        modelBuilder.Entity<JobApplication>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<JobApplication>()
            .Property(x => x.CandidateName)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<JobApplication>()
            .Property(x => x.CandidateEmail)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<JobApplication>()
            .Property(x => x.CandidatePhone)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<JobApplication>()
            .Property(x => x.Resume)
            .IsRequired();

        modelBuilder.Entity<JobApplication>()
            .HasMany(x => x.InterviewSchedules)
            .WithOne()
            .HasForeignKey(x => x.JobApplicationId);

        // InterviewSchedule configuration
        modelBuilder.Entity<InterviewSchedule>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<InterviewSchedule>()
            .Property(x => x.InterviewType)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<InterviewSchedule>()
            .Property(x => x.InterviewerName)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<InterviewSchedule>()
            .Property(x => x.InterviewerEmail)
            .IsRequired()
            .HasMaxLength(100);

        // OfferLetter configuration
        modelBuilder.Entity<OfferLetter>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OfferLetter>()
            .Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<OfferLetter>()
            .Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(200);

        // Outbox Message configuration
        modelBuilder.Entity<OutboxMessage>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OutboxMessage>()
            .Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<OutboxMessage>()
            .Property(x => x.EventData)
            .IsRequired();

        modelBuilder.Entity<OutboxMessage>()
            .HasIndex(x => new { x.CreatedAt, x.ProcessedAt })
            .HasName("idx_outbox_created_processed");
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
