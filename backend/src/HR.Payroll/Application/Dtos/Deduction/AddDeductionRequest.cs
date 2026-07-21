namespace HR.Payroll.Application.Dtos.Deduction;

/// <summary>
/// Deduction request DTO.
/// </summary>
public record AddDeductionRequest
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public string DeductionName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
