namespace HR.Payroll.Domain;

/// <summary>
/// Payroll record aggregate root for salary calculations.
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
    public string Status { get; set; } = "Draft"; // Draft, Processed, Approved, Paid
    public DateTime ProcessedDate { get; set; }
    public DateTime? PaidDate { get; set; }

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
            Status = "Draft",
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Calculate net salary from gross income and deductions.
    /// </summary>
    public void CalculatePayroll(decimal incomeTaxRate, decimal sscRate, decimal healthInsuranceAmount)
    {
        GrossIncome = BasicSalary + HousingAllowance + TransportAllowance + OtherAllowances;
        IncomeTax = (GrossIncome * incomeTaxRate) / 100m;
        SocialSecurityContribution = (GrossIncome * sscRate) / 100m;
        HealthInsurance = healthInsuranceAmount;
        NetSalary = GrossIncome - IncomeTax - SocialSecurityContribution - HealthInsurance;
        ProcessedDate = DateTime.UtcNow;
        Status = "Processed";
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
    /// Approve the payroll for payment.
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        Status = "Approved";
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
    /// Mark payroll as paid.
    /// </summary>
    public void MarkAsPaid()
    {
        Status = "Paid";
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

/// <summary>
/// Salary component (HRA, transport, etc.).
/// </summary>
public class SalaryComponent : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string ComponentName { get; set; } = string.Empty; // HRA, Transportation, etc.
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
    public DateTime EffectiveFromDate { get; set; }
    public DateTime? EffectiveToDate { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Deduction record (loans, advances, etc.).
/// </summary>
public class Deduction : AggregateRoot
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public string DeductionType { get; set; } = string.Empty; // Loan, Advance, Penalty, etc.
    public string DeductionName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DeductionDate { get; set; }

    public static Deduction Create(
        Guid payrollRecordId,
        Guid employeeId,
        string deductionType,
        string deductionName,
        decimal amount,
        Guid tenantId)
    {
        return new Deduction
        {
            Id = Guid.NewGuid(),
            PayrollRecordId = payrollRecordId,
            EmployeeId = employeeId,
            DeductionType = deductionType,
            DeductionName = deductionName,
            Amount = amount,
            DeductionDate = DateTime.UtcNow,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Tax calculation rules.
/// </summary>
public class TaxSlab : BaseEntity
{
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public decimal TaxRate { get; set; }
    public int Year { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Payslip for employee (read model, typically generated via Dapper).
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
}

// ===== DOMAIN EVENTS =====

public record PayrollCalculatedEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal NetSalary { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

public record PayrollApprovedEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ApprovedBy { get; set; }
}

public record PayrollPaidEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PaidDate { get; set; }
    public decimal Amount { get; set; }
}
