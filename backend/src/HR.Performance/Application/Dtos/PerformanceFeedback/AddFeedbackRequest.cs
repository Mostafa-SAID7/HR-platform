namespace HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Add performance feedback request DTO.
/// </summary>
public record AddFeedbackRequest
{
    public Guid FeedbackProviderId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int FeedbackRating { get; set; }
    public string FeedbackCategory { get; set; } = "General";
    public bool IsAnonymous { get; set; }
}
