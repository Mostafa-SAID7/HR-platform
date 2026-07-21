namespace HR.Performance.Domain.PerformanceReview.Events;

/// <summary>
/// Domain event raised when a performance review is approved
/// </summary>
public record PerformanceReviewApprovedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal PerformanceRating { get; set; }
}
