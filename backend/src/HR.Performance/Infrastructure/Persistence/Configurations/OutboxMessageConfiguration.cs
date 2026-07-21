namespace HR.Performance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Common.Domain;

/// <summary>
/// Entity Framework Core configuration for OutboxMessage.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> entity)
    {
        entity.ToTable("OutboxMessages");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.EventType).IsRequired();
        entity.Property(e => e.Content).IsRequired();
        entity.HasIndex(e => e.ProcessedOnUtc);
        entity.HasQueryFilter(e => !e.IsDeleted);
    }
}
