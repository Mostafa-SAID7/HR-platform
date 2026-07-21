namespace HR.Payroll.Domain.SalaryComponent;

/// <summary>
/// Salary component entity (HRA, transport, etc.)
/// </summary>
public class SalaryComponent : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string ComponentName { get; set; } = string.Empty; // HRA, Transportation, etc.
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
    public DateTime EffectiveFromDate { get; set; }
    public DateTime? EffectiveToDate { get; set; }
    public bool IsActive { get; set; } = true;

    private SalaryComponent() { }

    /// <summary>
    /// Create a new salary component
    /// </summary>
    public static SalaryComponent Create(
        Guid employeeId,
        string componentName,
        decimal amount,
        bool isDeduction,
        DateTime effectiveFromDate,
        Guid tenantId,
        DateTime? effectiveToDate = null)
    {
        return new SalaryComponent
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ComponentName = componentName,
            Amount = amount,
            IsDeduction = isDeduction,
            EffectiveFromDate = effectiveFromDate,
            EffectiveToDate = effectiveToDate,
            IsActive = true,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deactivate the salary component
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        EffectiveToDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Update component details
    /// </summary>
    public void Update(decimal amount, DateTime? effectiveToDate = null)
    {
        Amount = amount;
        EffectiveToDate = effectiveToDate;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
