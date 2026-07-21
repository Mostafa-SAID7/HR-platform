namespace HR.Notification.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Notification.Domain;
using HR.Notification.Infrastructure.Persistence.Configurations;
using HR.Notification.Infrastructure.Persistence.Seeds;
using HR.Common.Outbox;

/// <summary>
/// Database context for Notification Service.
/// Clean design: Configurations extracted into separate files.
/// </summary>
public class NotificationDbContext : DbContext, IUnitOfWork
{
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Seed initial data
        NotificationTemplateSeed.Seed(modelBuilder);
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
