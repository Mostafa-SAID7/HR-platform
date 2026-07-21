namespace HR.Payroll.Domain.PayrollRecord;

using HR.Payroll.Domain.PayrollRecord.Events;

/// <summary>
/// Payroll record aggregate root for salary calculations
/// </summary>
public class PayrollRecord : AggregateRoot
{
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
    public PayrollRecordStatus Status { get; set; }
    public DateTime ProcessedDate { get; set; }
    public DateTime? PaidDate { get; set; }

    private PayrollRecord() { }

    /// <summary>
    /// Create a new payroll record
    /// </summary>
    public static PayrollRecord Create(
        Guid employeeId,
        string employeeName,
        decimal basicSalary,
        int year,
        int month,
        Guid tenantId)
    {
        return new PayrollRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            BasicSalary = basicSalary,
            Year = year,
            Month = month,
            Status = PayrollRecordStatus.Draft,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Calculate net salary from gross income and deductions
    /// </summary>
    public void CalculatePayroll(decimal incomeTaxRate, decimal sscRate, decimal healthInsuranceAmount)
    {
        GrossIncome = BasicSalary + HousingAllowance + TransportAllowance + OtherAllowances;
        IncomeTax = (GrossIncome * incomeTaxRate) / 100m;
        SocialSecurityContribution = (GrossIncome * sscRate) / 100m;
        HealthInsurance = healthInsuranceAmount;
        NetSalary = GrossIncome - IncomeTax - SocialSecurityContribution - HealthInsurance;
        ProcessedDate = DateTime.UtcNow;
        Status = PayrollRecordStatus.Processed;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PayrollCalculatedEvent
        {
            PayrollRecordId = Id,
            EmployeeId = EmployeeId,
            GrossIncome = GrossIncome,
            NetSalary = NetSalary,
            Year = Year,
            Month = Month,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Approve the payroll for payment
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        if (Status != PayrollRecordStatus.Processed)
            throw new ValidationException("Only processed payroll records can be approved");

        Status = PayrollRecordStatus.Approved;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PayrollApprovedEvent
        {
            PayrollRecordId = Id,
            EmployeeId = EmployeeId,
            ApprovedBy = approvedBy,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Mark payroll as paid
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status != PayrollRecordStatus.Approved)
            throw new ValidationException("Only approved payroll records can be marked as paid");

        Status = PayrollRecordStatus.Paid;
        PaidDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PayrollPaidEvent
        {
            PayrollRecordId = Id,
            EmployeeId = EmployeeId,
            PaidDate = PaidDate.Value,
            Amount = NetSalary,
            TenantId = TenantId
        });
    }
}
