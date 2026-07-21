namespace HR.Recruitment.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Common.Outbox;

/// <summary>
/// Entity configuration for OutboxMessage
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.EventData)
            .IsRequired();

        builder.HasIndex(x => new { x.CreatedAt, x.ProcessedAt })
            .HasName("idx_outbox_created_processed");
    }
}
