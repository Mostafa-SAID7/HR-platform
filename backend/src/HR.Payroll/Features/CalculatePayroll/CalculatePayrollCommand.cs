namespace HR.Payroll.Features.CalculatePayroll;

using HR.Payroll.Application.Dtos.Payroll;

/// <summary>
/// Calculate payroll for an employee for a specific month/year
/// </summary>
public record CalculatePayrollCommand(
    CalculatePayrollRequest Request,
    Guid TenantId) : ICommand<PayrollRecordDto>;
