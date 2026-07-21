namespace HR.Payroll.Domain.PayrollRecord.Events;

/// <summary>
/// Domain event raised when payroll is paid
/// </summary>
public record PayrollPaidEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PaidDate { get; set; }
    public decimal Amount { get; set; }
}
