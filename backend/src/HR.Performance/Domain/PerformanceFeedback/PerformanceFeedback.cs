namespace HR.Performance.Domain.PerformanceFeedback;

using HR.Performance.Domain.PerformanceReview;

/// <summary>
/// Performance feedback entity - represents 360 feedback for a performance review
/// </summary>
public class PerformanceFeedback : BaseEntity
{
    public Guid PerformanceReviewId { get; set; }
    public Guid FeedbackProviderId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int FeedbackRating { get; set; } // 1-5
    public PerformanceFeedbackCategory Category { get; set; }
    public bool IsAnonymous { get; set; }

    public PerformanceReview? PerformanceReview { get; set; }

    private PerformanceFeedback() { }

    /// <summary>
    /// Create a new performance feedback
    /// </summary>
    public static PerformanceFeedback Create(
        Guid performanceReviewId,
        Guid feedbackProviderId,
        string comment,
        int feedbackRating,
        PerformanceFeedbackCategory category,
        bool isAnonymous = false,
        Guid? tenantId = null)
    {
        if (feedbackRating < 1 || feedbackRating > 5)
            throw new ValidationException("Feedback rating must be between 1 and 5");

        return new PerformanceFeedback
        {
            Id = Guid.NewGuid(),
            PerformanceReviewId = performanceReviewId,
            FeedbackProviderId = feedbackProviderId,
            Comment = comment,
            FeedbackRating = feedbackRating,
            Category = category,
            IsAnonymous = isAnonymous,
            TenantId = tenantId ?? Guid.Empty,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
