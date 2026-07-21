namespace HR.Audit.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Audit.Domain;

public class AuditReportConfiguration : IEntityTypeConfiguration<AuditReport>
{
    public void Configure(EntityTypeBuilder<AuditReport> builder)
    {
        builder.ToTable("AuditReports");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.GeneratedByUserId, e.GeneratedAt });
        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Type).HasMaxLength(100);
        builder.Property(e => e.Status).HasMaxLength(50);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
