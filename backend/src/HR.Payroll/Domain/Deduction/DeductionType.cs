namespace HR.Payroll.Domain.Deduction;

/// <summary>
/// Enum representing types of deductions
/// </summary>
public enum DeductionType
{
    Loan = 0,
    Advance = 1,
    Penalty = 2,
    TaxAdjustment = 3,
    Insurance = 4,
    OtherDeduction = 5
}
