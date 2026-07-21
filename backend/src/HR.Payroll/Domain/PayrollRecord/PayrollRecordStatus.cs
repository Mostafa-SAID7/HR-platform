namespace HR.Payroll.Domain.PayrollRecord;

/// <summary>
/// Enum representing payroll record status
/// </summary>
public enum PayrollRecordStatus
{
    Draft = 0,
    Processed = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}
