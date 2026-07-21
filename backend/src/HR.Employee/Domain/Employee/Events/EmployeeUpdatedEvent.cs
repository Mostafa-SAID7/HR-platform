namespace HR.Employee.Domain.Employee.Events;

/// <summary>
/// Domain event raised when an employee is updated
/// </summary>
public record EmployeeUpdatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
