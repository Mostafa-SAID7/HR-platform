namespace HR.Performance.Application.Dtos;

/// <summary>
/// Create performance review request DTO.
/// </summary>
public record CreatePerformanceReviewRequest
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
}

/// <summary>
/// Performance review list DTO.
/// </summary>
public record PerformanceReviewListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
    public decimal PerformanceRating { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

/// <summary>
/// Performance review detail DTO.
/// </summary>
public record PerformanceReviewDetailDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int ReviewYear { get; set; }
    public int ReviewQuarter { get; set; }
    public DateTime ReviewDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public decimal PerformanceRating { get; set; }
    public decimal ProductivityRating { get; set; }
    public decimal QualityRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    public decimal AverageRating { get; set; }

    public string Comments { get; set; } = string.Empty;
    public string StrengthAreas { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public bool IsFinal { get; set; }

    public List<PerformanceGoalDto> Goals { get; set; } = [];
    public List<PerformanceFeedbackDto> Feedback { get; set; } = [];
}

/// <summary>
/// Performance goal DTO.
/// </summary>
public record PerformanceGoalDto
{
    public Guid Id { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal CompletionPercentage { get; set; }
}

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

/// <summary>
/// Set performance ratings request DTO.
/// </summary>
public record SetPerformanceRatingsRequest
{
    public decimal PerformanceRating { get; set; }
    public decimal ProductivityRating { get; set; }
    public decimal QualityRating { get; set; }
    public decimal TeamworkRating { get; set; }
    public decimal LeadershipRating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string StrengthAreas { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
}

/// <summary>
/// Add feedback request DTO.
/// </summary>
public record AddFeedbackRequest
{
    public Guid FeedbackProviderId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int FeedbackRating { get; set; }
    public string FeedbackCategory { get; set; } = "General";
    public bool IsAnonymous { get; set; }
}

/// <summary>
/// Create goal request DTO.
/// </summary>
public record CreateGoalRequest
{
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Weight { get; set; }
}

/// <summary>
/// Performance filter DTO.
/// </summary>
public record PerformanceFilterDto : FilterDto
{
    public Guid? EmployeeId { get; set; }
    public int? ReviewYear { get; set; }
    public string? Status { get; set; }
    public decimal? MinRating { get; set; }
}
