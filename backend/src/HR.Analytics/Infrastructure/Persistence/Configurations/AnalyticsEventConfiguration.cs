namespace HR.Analytics.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Analytics.Domain;

/// <summary>
/// Entity Framework Core configuration for AnalyticsEvent aggregate.
/// Implements IEntityTypeConfiguration<T> for clean separation of concerns.
/// </summary>
public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    /// <summary>
    /// Configures the AnalyticsEvent entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("AnalyticsEvents");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.EventTimestamp);
        
        builder.Property(e => e.EventType).HasMaxLength(100);
        builder.Property(e => e.EntityType).HasMaxLength(100);
    }
}
