namespace HR.Notification.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Notification.Domain;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(255);
        builder.Property(x => x.RecipientEmail).HasMaxLength(100);
        builder.Property(x => x.RecipientPhone).HasMaxLength(20);
        builder.Property(x => x.Metadata).HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? []);
        builder.HasIndex(x => x.RecipientId).HasName("idx_notification_recipient_id");
        builder.HasIndex(x => x.Status).HasName("idx_notification_status");
        builder.HasIndex(x => new { x.CreatedAt, x.Status }).HasName("idx_notification_created_status");
    }
}
