namespace HR.Payroll.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Payroll.Domain;

/// <summary>
/// Seed data configuration for SalaryComponent aggregate
/// </summary>
public static class SalaryComponentSeed
{
    /// <summary>
    /// Seeds initial salary component data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var components = new List<object>();

        // Earnings components
        components.Add(CreateComponent(Guid.NewGuid(), "BasicSalary", "Basic Salary", SalaryComponentType.Earnings, 
            50000, true, "Fixed salary component", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "HouseRentAllowance", "House Rent Allowance (HRA)", SalaryComponentType.Earnings,
            10000, true, "Housing allowance - 20% of basic", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "DeartnessAllowance", "Dearness Allowance (DA)", SalaryComponentType.Earnings,
            5000, true, "Cost of living adjustment", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "MedicalAllowance", "Medical Allowance", SalaryComponentType.Earnings,
            2000, true, "Medical expense reimbursement", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "PerformanceBonus", "Performance Bonus", SalaryComponentType.Earnings,
            5000, false, "Variable - based on performance", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "TravelAllowance", "Travel Allowance", SalaryComponentType.Earnings,
            3000, true, "Travel expense coverage", tenantId));

        // Deductions components
        components.Add(CreateComponent(Guid.NewGuid(), "IncomeTax", "Income Tax (IT)", SalaryComponentType.Deductions,
            -8000, true, "Statutory income tax deduction", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "ProvidentFund", "Provident Fund (PF)", SalaryComponentType.Deductions,
            -3600, true, "Employee PF contribution (12% of basic)", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "EmployeeStatutoryInsurance", "Employee State Insurance (ESI)", SalaryComponentType.Deductions,
            -1200, true, "ESI contribution (0.75% of gross)", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "ProfessionalTax", "Professional Tax", SalaryComponentType.Deductions,
            -200, true, "Professional tax - varies by state", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "GroupInsurance", "Group Insurance", SalaryComponentType.Deductions,
            -500, true, "Group health insurance premium", tenantId));

        // Reimbursements
        components.Add(CreateComponent(Guid.NewGuid(), "LunchVoucher", "Lunch Voucher", SalaryComponentType.Reimbursement,
            2000, true, "Meal vouchers for employees", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "UniformAllowance", "Uniform Allowance", SalaryComponentType.Reimbursement,
            1000, true, "Annual uniform allowance", tenantId));

        components.Add(CreateComponent(Guid.NewGuid(), "BookAllowance", "Book Allowance", SalaryComponentType.Reimbursement,
            1500, true, "Professional development allowance", tenantId));

        modelBuilder.Entity<SalaryComponent>().HasData(components.ToArray());
    }

    private static object CreateComponent(Guid id, string code, string name, SalaryComponentType type,
        decimal defaultAmount, bool isActive, string description, Guid tenantId)
    {
        return new
        {
            Id = id,
            Code = code,
            Name = name,
            Type = (int)type,
            DefaultAmount = defaultAmount,
            IsActive = isActive,
            Description = description,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Salary component types for categorization
/// </summary>
public enum SalaryComponentType
{
    Earnings = 0,
    Deductions = 1,
    Reimbursement = 2
}
