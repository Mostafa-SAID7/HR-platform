namespace HR.Payroll.Application.Dtos.Payroll;

/// <summary>
/// Approve payroll request DTO.
/// </summary>
public record ApprovePayrollRequest
{
    public Guid PayrollRecordId { get; set; }
    public string? Comments { get; set; }
}
