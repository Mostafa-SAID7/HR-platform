namespace HR.Performance.Domain.PerformanceReview;

using HR.Performance.Domain.PerformanceReview.Events;
using HR.Performance.Domain.PerformanceFeedback;

/// <summary>
/// Performance review aggregate root
/// </summary>
public class PerformanceReview : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; } // 1-4
    public DateTime ReviewDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Ratings (1-5 scale)
    public decimal PerformanceRating { get; set; } // Overall performance
    public decimal ProductivityRating { get; set; }
    public decimal QualityRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    
    public string Comments { get; set; } = string.Empty;
    public string StrengthAreas { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
    
    public PerformanceReviewStatus Status { get; set; }
    public bool IsFinal { get; set; }
    
    // Relations
    public ICollection<PerformanceGoal> Goals { get; set; } = new List<PerformanceGoal>();
    public ICollection<PerformanceFeedback> Feedback { get; set; } = new List<PerformanceFeedback>();

    private PerformanceReview() { }

    /// <summary>
    /// Create a new performance review
    /// </summary>
    public static PerformanceReview Create(
        Guid employeeId,
        Guid reviewerId,
        string employeeName,
        string reviewerName,
        int reviewYear,
        int reviewQuarter,
        Guid tenantId)
    {
        var review = new PerformanceReview
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ReviewerId = reviewerId,
            EmployeeName = employeeName,
            ReviewerName = reviewerName,
            ReviewYear = reviewYear,
            ReviewQuarter = reviewQuarter,
            ReviewDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            Status = PerformanceReviewStatus.Draft
        };

        // Raise domain event
        review.AddDomainEvent(new PerformanceReviewCreatedEvent
        {
            PerformanceReviewId = review.Id,
            EmployeeId = employeeId,
            ReviewYear = reviewYear,
            TenantId = tenantId
        });

        return review;
    }

    /// <summary>
    /// Submit the performance review
    /// </summary>
    public void Submit()
    {
        if (Status != PerformanceReviewStatus.Draft)
            throw new BusinessRuleViolationException("ReviewNotInDraft", "Only draft reviews can be submitted");

        Status = PerformanceReviewStatus.Submitted;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PerformanceReviewSubmittedEvent
        {
            PerformanceReviewId = Id,
            EmployeeId = EmployeeId,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Approve the performance review
    /// </summary>
    public void Approve()
    {
        if (Status != PerformanceReviewStatus.Submitted)
            throw new BusinessRuleViolationException("ReviewNotSubmitted", "Only submitted reviews can be approved");

        Status = PerformanceReviewStatus.Approved;
        IsFinal = true;
        CompletedDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PerformanceReviewApprovedEvent
        {
            PerformanceReviewId = Id,
            EmployeeId = EmployeeId,
            PerformanceRating = PerformanceRating,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Set performance ratings
    /// </summary>
    public void SetRatings(
        decimal performanceRating,
        decimal productivityRating,
        decimal qualityRating,
        decimal teamworkRating,
        decimal leadershipRating)
    {
        if (performanceRating is < 1 or > 5 || productivityRating is < 1 or > 5 ||
            qualityRating is < 1 or > 5 || teamworkRating is < 1 or > 5 ||
            leadershipRating is < 1 or > 5)
        {
            throw new ValidationException("All ratings must be between 1 and 5");
        }

        PerformanceRating = performanceRating;
        ProductivityRating = productivityRating;
        QualityRating = qualityRating;
        TeamworkRating = teamworkRating;
        LeadershipRating = leadershipRating;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Add feedback to the review
    /// </summary>
    public void AddFeedback(Guid feedbackProviderId, string feedbackComment)
    {
        var feedback = new PerformanceFeedback
        {
            Id = Guid.NewGuid(),
            PerformanceReviewId = Id,
            FeedbackProviderId = feedbackProviderId,
            Comment = feedbackComment,
            TenantId = TenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        Feedback.Add(feedback);
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate average rating
    /// </summary>
    public decimal GetAverageRating()
    {
        if (PerformanceRating == 0) return 0;
        return (PerformanceRating + ProductivityRating + QualityRating + TeamworkRating + LeadershipRating) / 5m;
    }
}
