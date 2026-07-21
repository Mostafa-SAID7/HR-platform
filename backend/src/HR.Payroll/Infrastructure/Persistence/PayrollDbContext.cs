namespace HR.Payroll.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Payroll.Domain;
using HR.Payroll.Infrastructure.Persistence.Configurations;
using HR.Common.Outbox;

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

        modelBuilder.ApplyConfiguration(new PayrollRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryComponentConfiguration());
        modelBuilder.ApplyConfiguration(new DeductionConfiguration());
        modelBuilder.ApplyConfiguration(new TaxSlabConfiguration());
        modelBuilder.ApplyConfiguration(new PayslipConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}
