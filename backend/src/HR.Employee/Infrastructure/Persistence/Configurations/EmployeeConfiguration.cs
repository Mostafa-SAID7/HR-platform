namespace HR.Employee.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Employee.Domain.Employee;

/// <summary>
/// Entity configuration for Employee aggregate
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
        builder.HasIndex(e => new { e.NationalId, e.TenantId }).IsUnique();
        
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.Gender).HasMaxLength(10);
        builder.Property(e => e.NationalId).IsRequired().HasMaxLength(50);
        builder.Property(e => e.JobTitle).HasMaxLength(256);
        builder.Property(e => e.EmploymentType).HasMaxLength(50);
        builder.Property(e => e.Salary).HasPrecision(19, 2);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(50);
        
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Skills)
            .WithOne(s => s.Employee)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Education)
            .WithOne(ed => ed.Employee)
            .HasForeignKey(ed => ed.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
