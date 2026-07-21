namespace HR.Performance.Domain;

/// <summary>
/// Performance review aggregate root.
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
    
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
    public bool IsFinal { get; set; }
    
    // Relations
    public ICollection<PerformanceGoal> Goals { get; set; } = new List<PerformanceGoal>();
    public ICollection<PerformanceFeedback> Feedback { get; set; } = new List<PerformanceFeedback>();

    /// <summary>
    /// Create a new performance review.
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
            Status = "Draft"
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
    /// Submit the performance review.
    /// </summary>
    public void Submit()
    {
        if (Status != "Draft")
            throw new BusinessRuleViolationException("ReviewNotInDraft", "Only draft reviews can be submitted");

        Status = "Submitted";
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new PerformanceReviewSubmittedEvent
        {
            PerformanceReviewId = Id,
            EmployeeId = EmployeeId,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Approve the performance review.
    /// </summary>
    public void Approve()
    {
        if (Status != "Submitted")
            throw new BusinessRuleViolationException("ReviewNotSubmitted", "Only submitted reviews can be approved");

        Status = "Approved";
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
    /// Set performance ratings.
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
    /// Add feedback to the review.
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
    /// Calculate average rating.
    /// </summary>
    public decimal GetAverageRating()
    {
        if (PerformanceRating == 0) return 0;
        return (PerformanceRating + ProductivityRating + QualityRating + TeamworkRating + LeadershipRating) / 5m;
    }
}

/// <summary>
/// Performance goal entity.
/// </summary>
public class PerformanceGoal : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
    public decimal Weight { get; set; } // 0-100, percentage contribution to review

    public PerformanceReview? PerformanceReview { get; set; }

    /// <summary>
    /// Create a new performance goal.
    /// </summary>
    public static PerformanceGoal Create(
        Guid employeeId,
        Guid reviewId,
        string title,
        string description,
        decimal targetValue,
        string unitOfMeasure,
        DateTime dueDate,
        decimal weight,
        Guid tenantId)
    {
        return new PerformanceGoal
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PerformanceReviewId = reviewId,
            GoalTitle = title,
            GoalDescription = description,
            TargetValue = targetValue,
            UnitOfMeasure = unitOfMeasure,
            StartDate = DateTime.UtcNow,
            DueDate = dueDate,
            Weight = weight,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update goal progress.
    /// </summary>
    public void UpdateProgress(decimal actualValue)
    {
        ActualValue = actualValue;
        UpdatedOnUtc = DateTime.UtcNow;

        if (actualValue >= TargetValue)
        {
            Status = "Completed";
        }
    }

    /// <summary>
    /// Get goal completion percentage.
    /// </summary>
    public decimal GetCompletionPercentage()
    {
        if (TargetValue == 0) return 0;
        return (ActualValue / TargetValue) * 100m;
    }
}

/// <summary>
/// Performance feedback entity (360 feedback).
/// </summary>
public class PerformanceFeedback : BaseEntity
{
    public Guid PerformanceReviewId { get; set; }
    public Guid FeedbackProviderId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int FeedbackRating { get; set; } // 1-5
    public string FeedbackCategory { get; set; } = "General"; // Communication, Leadership, Teamwork, etc.
    public bool IsAnonymous { get; set; }

    public PerformanceReview? PerformanceReview { get; set; }
}

// ===== DOMAIN EVENTS =====

/// <summary>
/// Event raised when a performance review is created.
/// </summary>
public record PerformanceReviewCreatedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
    public int ReviewYear { get; set; }
}

/// <summary>
/// Event raised when a performance review is submitted.
/// </summary>
public record PerformanceReviewSubmittedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
}

/// <summary>
/// Event raised when a performance review is approved.
/// </summary>
public record PerformanceReviewApprovedEvent : DomainEvent
{
    public Guid PerformanceReviewId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal PerformanceRating { get; set; }
}

/// <summary>
/// Event raised when a performance goal is completed.
/// </summary>
public record PerformanceGoalCompletedEvent : DomainEvent
{
    public Guid GoalId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal CompletionPercentage { get; set; }
}
