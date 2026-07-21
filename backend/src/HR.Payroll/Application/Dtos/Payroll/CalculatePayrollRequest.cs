namespace HR.Payroll.Application.Dtos.Payroll;

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
    public string? EmployeeName { get; set; }
}
