namespace HR.Employee.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Employee.Domain.Department;

/// <summary>
/// Entity configuration for Department aggregate
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Location).HasMaxLength(256);
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
