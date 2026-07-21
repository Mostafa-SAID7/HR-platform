namespace HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Domain event: Employee Deleted.
/// SOLID: Event definition in separate file.
/// </summary>
public class EmployeeDeletedEvent
{
    public Guid EmployeeId { get; set; }
    public DateTime DeletedAt { get; set; }
}
