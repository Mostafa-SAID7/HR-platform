namespace HR.Employee.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Employee.Domain.EmployeeEducation;

/// <summary>
/// Entity configuration for EmployeeEducation aggregate
/// </summary>
public class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
{
    public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
    {
        builder.ToTable("EmployeeEducation");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.InstitutionName).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Degree).IsRequired().HasMaxLength(256);
        builder.Property(e => e.FieldOfStudy).IsRequired().HasMaxLength(256);
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
