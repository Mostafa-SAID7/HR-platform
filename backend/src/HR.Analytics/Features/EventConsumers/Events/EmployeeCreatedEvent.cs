namespace HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Domain event: Employee Created.
/// SOLID: Event definition in separate file.
/// </summary>
public class EmployeeCreatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
