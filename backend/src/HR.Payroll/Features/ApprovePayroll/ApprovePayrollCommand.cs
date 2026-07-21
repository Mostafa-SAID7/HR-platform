namespace HR.Payroll.Features.ApprovePayroll;

/// <summary>
/// Approve a processed payroll record
/// </summary>
public record ApprovePayrollCommand(
    Guid PayrollRecordId,
    Guid ApprovedBy,
    Guid TenantId) : ICommand;
