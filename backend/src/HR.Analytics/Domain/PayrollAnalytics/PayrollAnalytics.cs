namespace HR.Analytics.Domain.PayrollAnalytics;

/// <summary>
/// Payroll analytics view - denormalized from Payroll Service
/// Provides payroll reporting and analysis data
/// </summary>
public class PayrollAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
}
