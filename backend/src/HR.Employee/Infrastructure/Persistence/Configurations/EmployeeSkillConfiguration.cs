namespace HR.Employee.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HR.Employee.Domain.EmployeeSkill;

/// <summary>
/// Entity configuration for EmployeeSkill aggregate
/// </summary>
public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> builder)
    {
        builder.ToTable("EmployeeSkills");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.EmployeeId, e.SkillName }).IsUnique();
        
        builder.Property(e => e.SkillName).IsRequired().HasMaxLength(256);
        
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
