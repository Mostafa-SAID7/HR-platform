namespace HR.Employee.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Employee.Domain.Employee;
using HR.Employee.Domain.Department;
using HR.Employee.Domain.EmployeeSkill;
using HR.Employee.Domain.EmployeeEducation;
using HR.Common.Domain.Events;
using HR.Employee.Infrastructure.Persistence.Configurations;
using HR.Employee.Infrastructure.Persistence.Seeds;

/// <summary>
/// Entity Framework Core database context for Employee Service.
/// Applies entity configurations from separate configuration classes following SOLID principles
/// Seeds initial data from dedicated seed classes
/// </summary>
public class EmployeeDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; } = null!;
    public DbSet<EmployeeEducation> EmployeeEducation { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from separate classes
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSkillConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEducationConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // Apply seed data from dedicated seed classes
        EmployeeSeed.Seed(modelBuilder);
        DepartmentSeed.Seed(modelBuilder);
    }
}
