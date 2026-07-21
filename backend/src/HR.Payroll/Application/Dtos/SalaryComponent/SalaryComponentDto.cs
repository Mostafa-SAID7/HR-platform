namespace HR.Payroll.Application.Dtos.SalaryComponent;

/// <summary>
/// Salary component DTO.
/// </summary>
public record SalaryComponentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EffectiveFromDate { get; set; }
}
