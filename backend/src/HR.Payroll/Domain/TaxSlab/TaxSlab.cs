namespace HR.Payroll.Domain.TaxSlab;

/// <summary>
/// Tax slab entity for tax calculation rules
/// </summary>
public class TaxSlab : BaseEntity
{
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public decimal TaxRate { get; set; }
    public int Year { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    private TaxSlab() { }

    /// <summary>
    /// Create a new tax slab
    /// </summary>
    public static TaxSlab Create(
        decimal minSalary,
        decimal maxSalary,
        decimal taxRate,
        int year,
        string description,
        Guid tenantId)
    {
        if (minSalary < 0 || maxSalary < 0 || minSalary > maxSalary)
            throw new ValidationException("Invalid salary range");

        if (taxRate < 0 || taxRate > 100)
            throw new ValidationException("Tax rate must be between 0 and 100");

        return new TaxSlab
        {
            Id = Guid.NewGuid(),
            MinSalary = minSalary,
            MaxSalary = maxSalary,
            TaxRate = taxRate,
            Year = year,
            Description = description,
            IsActive = true,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Check if salary falls within this tax slab
    /// </summary>
    public bool IsSalaryInRange(decimal salary)
    {
        return salary >= MinSalary && salary <= MaxSalary && IsActive;
    }

    /// <summary>
    /// Calculate tax for given salary
    /// </summary>
    public decimal CalculateTax(decimal salary)
    {
        return (salary * TaxRate) / 100m;
    }
}
