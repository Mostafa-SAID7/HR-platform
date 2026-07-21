namespace HR.Performance.Application.Dtos.PerformanceReview;

using HR.Performance.Application.Dtos.PerformanceGoal;
using HR.Performance.Application.Dtos.PerformanceFeedback;

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
