namespace HR.Payroll.Domain.PayrollRecord.Events;

/// <summary>
/// Domain event raised when payroll is approved
/// </summary>
public record PayrollApprovedEvent : DomainEvent
{
    public Guid PayrollRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ApprovedBy { get; set; }
}
