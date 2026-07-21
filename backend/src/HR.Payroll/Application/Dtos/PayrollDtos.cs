namespace HR.Payroll.Application.Dtos;

/// <summary>
/// Payroll calculation request DTO.
/// </summary>
public record CalculatePayrollRequest
{
    public Guid EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; } = 0;
    public decimal TransportAllowance { get; set; } = 0;
    public decimal OtherAllowances { get; set; } = 0;
    public int Year { get; set; }
    public int Month { get; set; }
}

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

/// <summary>
/// Payroll record DTO.
/// </summary>
public record PayrollRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal SocialSecurityContribution { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
    public DateTime? PaidDate { get; set; }
}

/// <summary>
/// Payslip DTO.
/// </summary>
public record PayslipDto
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal SocialSecurityContribution { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime IssuedDate { get; set; }
}

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
