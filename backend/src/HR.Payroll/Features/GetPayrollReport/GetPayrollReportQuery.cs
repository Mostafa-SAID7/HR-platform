namespace HR.Payroll.Features.GetPayrollReport;

using HR.Payroll.Application.Dtos.Report;

/// <summary>
/// Get payroll report for a specific month/year
/// </summary>
public record GetPayrollReportQuery(
    int Year,
    int Month,
    Guid TenantId) : IQuery<PayrollReportDto>;
