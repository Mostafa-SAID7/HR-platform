namespace HR.Payroll.Domain.Payslip;

/// <summary>
/// Payslip entity for employee (read model, typically generated via Dapper)
/// </summary>
public class Payslip : BaseEntity
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string PayslipContent { get; set; } = string.Empty; // JSON or HTML payslip
    public DateTime IssuedDate { get; set; }

    private Payslip() { }

    /// <summary>
    /// Create a new payslip
    /// </summary>
    public static Payslip Create(
        Guid payrollRecordId,
        Guid employeeId,
        string employeeName,
        int year,
        int month,
        string payslipContent,
        Guid tenantId)
    {
        return new Payslip
        {
            Id = Guid.NewGuid(),
            PayrollRecordId = payrollRecordId,
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            Year = year,
            Month = month,
            PayslipContent = payslipContent,
            IssuedDate = DateTime.UtcNow,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update payslip content
    /// </summary>
    public void UpdateContent(string payslipContent)
    {
        PayslipContent = payslipContent;
        LastModifiedOnUtc = DateTime.UtcNow;
    }
}
