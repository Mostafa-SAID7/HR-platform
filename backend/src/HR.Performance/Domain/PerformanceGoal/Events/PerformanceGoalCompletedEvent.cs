namespace HR.Performance.Domain.PerformanceGoal.Events;

/// <summary>
/// Domain event raised when a performance goal is completed
/// </summary>
public record PerformanceGoalCompletedEvent : DomainEvent
{
    public Guid GoalId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal CompletionPercentage { get; set; }
}
