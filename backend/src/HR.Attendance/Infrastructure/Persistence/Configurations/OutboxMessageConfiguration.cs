namespace HR.Attendance.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Common.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.Content).IsRequired();
        builder.HasIndex(e => e.ProcessedOnUtc);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
