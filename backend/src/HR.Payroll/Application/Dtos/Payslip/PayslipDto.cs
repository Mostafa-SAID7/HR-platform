namespace HR.Payroll.Application.Dtos.Payslip;

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
