namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Approve performance review request DTO.
/// </summary>
public record ApprovePerformanceReviewRequest
{
    public Guid ReviewId { get; set; }
}
