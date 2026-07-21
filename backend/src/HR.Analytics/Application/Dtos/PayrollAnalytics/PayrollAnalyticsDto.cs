namespace HR.Analytics.Application.Dtos.PayrollAnalytics;

/// <summary>
/// Payroll analytics DTO.
/// </summary>
public record PayrollAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
}
