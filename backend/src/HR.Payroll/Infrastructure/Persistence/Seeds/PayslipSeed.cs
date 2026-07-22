namespace HR.Payroll.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Payroll.Domain;

/// <summary>
/// Seed data configuration for Payslip aggregate
/// </summary>
public static class PayslipSeed
{
    /// <summary>
    /// Seeds initial payslip data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var payslips = new List<object>();

        // Sample payslips for testing (current month)
        var currentDate = DateTime.UtcNow;
        var payslipMonth = currentDate.Month;
        var payslipYear = currentDate.Year;

        // Employee 1 payslip
        payslips.Add(CreatePayslip(
            Guid.NewGuid(),
            Guid.Parse("00000000-0000-0000-0000-000000000101"), // Sample Employee ID
            payslipMonth,
            payslipYear,
            75000, // Gross salary
            8000,  // Tax
            3600,  // PF
            1200,  // ESI
            62200, // Net salary
            "Generated",
            tenantId
        ));

        // Employee 2 payslip
        payslips.Add(CreatePayslip(
            Guid.NewGuid(),
            Guid.Parse("00000000-0000-0000-0000-000000000102"),
            payslipMonth,
            payslipYear,
            65000,
            6500,
            3100,
            1000,
            54400,
            "Generated",
            tenantId
        ));

        // Employee 3 payslip
        payslips.Add(CreatePayslip(
            Guid.NewGuid(),
            Guid.Parse("00000000-0000-0000-0000-000000000103"),
            payslipMonth,
            payslipYear,
            85000,
            10000,
            4200,
            1300,
            69500,
            "Generated",
            tenantId
        ));

        modelBuilder.Entity<Payslip>().HasData(payslips.ToArray());
    }

    private static object CreatePayslip(Guid id, Guid employeeId, int month, int year,
        decimal grossSalary, decimal taxDeduction, decimal pfDeduction, decimal esiDeduction,
        decimal netSalary, string status, Guid tenantId)
    {
        return new
        {
            Id = id,
            EmployeeId = employeeId,
            PayslipMonth = month,
            PayslipYear = year,
            GrossSalary = grossSalary,
            TaxDeduction = taxDeduction,
            PfDeduction = pfDeduction,
            EsiDeduction = esiDeduction,
            OtherDeductions = 0m,
            NetSalary = netSalary,
            Status = status,
            GeneratedOn = DateTime.UtcNow,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }
}
