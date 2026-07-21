namespace HR.Employee.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Employee.Domain;

/// <summary>
/// Entity Framework Core database context for Employee Service.
/// </summary>
public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; } = null!;
    public DbSet<EmployeeEducation> EmployeeEducation { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
            entity.HasIndex(e => new { e.NationalId, e.TenantId }).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.NationalId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(256);
            entity.Property(e => e.EmploymentType).HasMaxLength(50);
            entity.Property(e => e.Salary).HasPrecision(19, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Skills)
                .WithOne(s => s.Employee)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Education)
                .WithOne(ed => ed.Employee)
                .HasForeignKey(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Department entity
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(256);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure EmployeeSkill
        modelBuilder.Entity<EmployeeSkill>(entity =>
        {
            entity.ToTable("EmployeeSkills");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.SkillName }).IsUnique();
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(256);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure EmployeeEducation
        modelBuilder.Entity<EmployeeEducation>(entity =>
        {
            entity.ToTable("EmployeeEducation");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InstitutionName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Degree).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FieldOfStudy).IsRequired().HasMaxLength(256);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Outbox
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ProcessedOnUtc);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
