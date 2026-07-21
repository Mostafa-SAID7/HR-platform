namespace HR.Payroll.Domain.PayrollRecord.Events;

/// <summary>
/// Domain event raised when payroll is calculated
/// </summary>
public record PayrollCalculatedEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal NetSalary { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}
