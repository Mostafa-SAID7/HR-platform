namespace HR.Payroll.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Payroll.Domain;

/// <summary>
/// Entity Framework Core database context for Payroll Service.
/// </summary>
public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options) { }

    public DbSet<PayrollRecord> PayrollRecords { get; set; } = null!;
    public DbSet<SalaryComponent> SalaryComponents { get; set; } = null!;
    public DbSet<Deduction> Deductions { get; set; } = null!;
    public DbSet<TaxSlab> TaxSlabs { get; set; } = null!;
    public DbSet<Payslip> Payslips { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PayrollRecord
        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.ToTable("PayrollRecords");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month, e.TenantId }).IsUnique();
            entity.Property(e => e.EmployeeName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.GrossIncome).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure SalaryComponent
        modelBuilder.Entity<SalaryComponent>(entity =>
        {
            entity.ToTable("SalaryComponents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveFromDate, e.TenantId });
            entity.Property(e => e.ComponentName).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Deduction
        modelBuilder.Entity<Deduction>(entity =>
        {
            entity.ToTable("Deductions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PayrollRecordId, e.TenantId });
            entity.Property(e => e.DeductionType).HasMaxLength(100);
            entity.Property(e => e.DeductionName).HasMaxLength(256);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure TaxSlab
        modelBuilder.Entity<TaxSlab>(entity =>
        {
            entity.ToTable("TaxSlabs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Year, e.MinSalary });
            entity.Property(e => e.MinSalary).HasPrecision(18, 2);
            entity.Property(e => e.MaxSalary).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
        });

        // Configure Payslip
        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.ToTable("Payslips");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month });
            entity.Property(e => e.PayslipContent).HasColumnType("text");
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
