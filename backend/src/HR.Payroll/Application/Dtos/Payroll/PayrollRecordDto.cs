namespace HR.Payroll.Application.Dtos.Payroll;

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
