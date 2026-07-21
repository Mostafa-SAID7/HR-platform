namespace HR.Performance.Domain.PerformanceReview.Events;

/// <summary>
/// Domain event raised when a performance review is submitted
/// </summary>
public record PerformanceReviewSubmittedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
}
