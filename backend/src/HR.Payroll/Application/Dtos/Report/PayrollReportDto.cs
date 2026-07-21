namespace HR.Payroll.Application.Dtos.Report;

/// <summary>
/// Payroll report for bulk processing.
/// </summary>
public record PayrollReportDto
{
    public int TotalEmployees { get; set; }
    public int ProcessedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public decimal TotalGrossIncome { get; set; }
    public decimal TotalIncomeTax { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public DateTime ReportGeneratedDate { get; set; }
}
