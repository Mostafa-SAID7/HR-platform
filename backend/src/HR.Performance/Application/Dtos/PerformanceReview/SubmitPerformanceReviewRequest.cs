namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Submit performance review request DTO.
/// </summary>
public record SubmitPerformanceReviewRequest
{
    public Guid ReviewId { get; set; }
}
