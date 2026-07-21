namespace HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Performance feedback DTO.
/// </summary>
public record PerformanceFeedbackDto
{
    public Guid Id { get; set; }
    public Guid FeedbackProviderId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int FeedbackRating { get; set; }
    public string FeedbackCategory { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
