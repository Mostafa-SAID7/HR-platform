namespace HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Submit performance review request DTO.
/// </summary>
public record SubmitPerformanceReviewRequest
{
    public string Comments { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
}
