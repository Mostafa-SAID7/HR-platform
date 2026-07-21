namespace HR.Payroll.Application.Dtos.Report;

/// <summary>
/// Employee salary slip summary.
/// </summary>
public record EmployeeSalarySummaryDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal YearToDateGrossIncome { get; set; }
    public decimal YearToDateIncomeTax { get; set; }
    public decimal YearToDateNetSalary { get; set; }
    public int PayslipsProcessed { get; set; }
}
