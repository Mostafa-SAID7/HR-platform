namespace HR.Notification.Infrastructure.Persistence;

/// <summary>
/// Database context for Notification Service
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

        // Notification configuration
        modelBuilder.Entity<Notification>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Notification>()
            .Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Notification>()
            .Property(x => x.RecipientEmail)
            .HasMaxLength(100);

        modelBuilder.Entity<Notification>()
            .Property(x => x.RecipientPhone)
            .HasMaxLength(20);

        modelBuilder.Entity<Notification>()
            .Property(x => x.Metadata)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? []);

        modelBuilder.Entity<Notification>()
            .HasIndex(x => x.RecipientId)
            .HasName("idx_notification_recipient_id");

        modelBuilder.Entity<Notification>()
            .HasIndex(x => x.Status)
            .HasName("idx_notification_status");

        modelBuilder.Entity<Notification>()
            .HasIndex(x => new { x.CreatedAt, x.Status })
            .HasName("idx_notification_created_status");

        // NotificationTemplate configuration
        modelBuilder.Entity<NotificationTemplate>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<NotificationTemplate>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<NotificationTemplate>()
            .Property(x => x.Description)
            .HasMaxLength(500);

        modelBuilder.Entity<NotificationTemplate>()
            .Property(x => x.TitleTemplate)
            .IsRequired();

        modelBuilder.Entity<NotificationTemplate>()
            .Property(x => x.ContentTemplate)
            .IsRequired();

        modelBuilder.Entity<NotificationTemplate>()
            .Property(x => x.VariableMappings)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? []);

        // NotificationPreference configuration
        modelBuilder.Entity<NotificationPreference>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<NotificationPreference>()
            .HasIndex(x => x.UserId)
            .HasName("idx_preference_user_id")
            .IsUnique();

        modelBuilder.Entity<NotificationPreference>()
            .Property(x => x.TypePreferences)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<NotificationType, NotificationChannelPreference>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? []);

        // Outbox Message configuration
        modelBuilder.Entity<OutboxMessage>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OutboxMessage>()
            .Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(255);

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
