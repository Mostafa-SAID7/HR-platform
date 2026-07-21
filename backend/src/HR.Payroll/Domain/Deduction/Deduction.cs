namespace HR.Payroll.Domain.Deduction;

/// <summary>
/// Deduction record entity (loans, advances, etc.)
/// </summary>
public class Deduction : AggregateRoot
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DeductionType DeductionType { get; set; }
    public string DeductionName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DeductionDate { get; set; }

    private Deduction() { }

    /// <summary>
    /// Create a new deduction record
    /// </summary>
    public static Deduction Create(
        Guid payrollRecordId,
        Guid employeeId,
        DeductionType deductionType,
        string deductionName,
        decimal amount,
        string description,
        Guid tenantId)
    {
        if (amount <= 0)
            throw new ValidationException("Deduction amount must be greater than zero");

        return new Deduction
        {
            Id = Guid.NewGuid(),
            PayrollRecordId = payrollRecordId,
            EmployeeId = employeeId,
            DeductionType = deductionType,
            DeductionName = deductionName,
            Amount = amount,
            Description = description,
            DeductionDate = DateTime.UtcNow,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
