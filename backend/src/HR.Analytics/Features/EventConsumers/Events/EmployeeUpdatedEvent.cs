namespace HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Domain event: Employee Updated.
/// SOLID: Event definition in separate file.
/// </summary>
public class EmployeeUpdatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
