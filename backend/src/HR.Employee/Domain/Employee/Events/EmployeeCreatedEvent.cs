namespace HR.Employee.Domain.Employee.Events;

/// <summary>
/// Domain event raised when an employee is created
/// </summary>
public record EmployeeCreatedEvent : DomainEvent
{
    public Guid EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
