namespace HR.Payroll.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Payroll.Domain;

/// <summary>
/// Seed data configuration for TaxSlab aggregate
/// </summary>
public static class TaxSlabSeed
{
    /// <summary>
    /// Seeds initial tax slab data for India FY 2023-24
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var taxSlabs = new List<object>();

        // Indian income tax slabs for FY 2023-24 (Regular)
        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab0", "0% Tax Slab", 0, 300000, 0, 
            "No tax for income up to ₹3,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab5", "5% Tax Slab", 300000, 600000, 0.05m,
            "5% tax on income from ₹3,00,001 to ₹6,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab10", "10% Tax Slab", 600000, 900000, 0.10m,
            "10% tax on income from ₹6,00,001 to ₹9,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab15", "15% Tax Slab", 900000, 1200000, 0.15m,
            "15% tax on income from ₹9,00,001 to ₹12,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab20", "20% Tax Slab", 1200000, 1500000, 0.20m,
            "20% tax on income from ₹12,00,001 to ₹15,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "Slab30", "30% Tax Slab", 1500000, decimal.MaxValue, 0.30m,
            "30% tax on income above ₹15,00,000", tenantId));

        // Senior Citizen slabs (Age >= 60)
        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "SeniorCitizenSlab0", "Senior Citizen 0% Tax Slab", 0, 500000, 0,
            "No tax for senior citizens up to ₹5,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "SeniorCitizenSlab5", "Senior Citizen 5% Tax Slab", 500000, 1000000, 0.05m,
            "5% tax for senior citizens from ₹5,00,001 to ₹10,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "SeniorCitizenSlab20", "Senior Citizen 20% Tax Slab", 1000000, decimal.MaxValue, 0.20m,
            "20% tax for senior citizens above ₹10,00,000", tenantId));

        // Super Senior Citizen slabs (Age >= 80)
        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "SuperSeniorSlab0", "Super Senior Citizen 0% Tax Slab", 0, 500000, 0,
            "No tax for super senior citizens up to ₹5,00,000", tenantId));

        taxSlabs.Add(CreateTaxSlab(Guid.NewGuid(), "SuperSeniorSlab20", "Super Senior Citizen 20% Tax Slab", 500000, decimal.MaxValue, 0.20m,
            "20% tax for super senior citizens above ₹5,00,000", tenantId));

        modelBuilder.Entity<TaxSlab>().HasData(taxSlabs.ToArray());
    }

    private static object CreateTaxSlab(Guid id, string code, string name, decimal minAmount, decimal maxAmount,
        decimal taxRate, string description, Guid tenantId)
    {
        return new
        {
            Id = id,
            Code = code,
            Name = name,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            TaxRate = taxRate,
            Description = description,
            IsActive = true,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }
}
