namespace HR.Payroll.Features.GetPayslip;

using HR.Payroll.Application.Dtos.Payslip;

/// <summary>
/// Retrieve a payslip for a payroll record
/// </summary>
public record GetPayslipQuery(
    Guid PayrollRecordId,
    Guid TenantId) : IQuery<PayslipDto>;
