namespace HR.Employee.Domain.Employee.Events;

/// <summary>
/// Domain event raised when an employee is terminated
/// </summary>
public record EmployeeTerminatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime TerminationDate { get; set; }
}
