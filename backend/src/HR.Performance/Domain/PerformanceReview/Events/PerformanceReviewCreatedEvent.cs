namespace HR.Performance.Domain.PerformanceReview.Events;

/// <summary>
/// Domain event raised when a performance review is created
/// </summary>
public record PerformanceReviewCreatedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
    public int ReviewYear { get; set; }
}
