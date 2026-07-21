namespace HR.Performance.Application.Dtos.PerformanceReview.Mappings;

using HR.Performance.Domain;
using HR.Performance.Application.Dtos.PerformanceGoal;
using HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Centralized mapping configuration for PerformanceReview DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class PerformanceReviewDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceReview domain entity to PerformanceReviewDetailDto.
    /// </summary>
    public static PerformanceReviewDetailDto ToDetailDto(this PerformanceReview review)
    {
        return new PerformanceReviewDetailDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            ReviewerId = review.ReviewerId,
            EmployeeName = review.EmployeeName,
            ReviewerName = review.ReviewerName,
            ReviewYear = review.ReviewYear,
            ReviewQuarter = review.ReviewQuarter,
            ReviewDate = review.ReviewDate,
            DueDate = review.DueDate,
            CompletedDate = review.CompletedDate,
            PerformanceRating = review.PerformanceRating,
            ProductivityRating = review.ProductivityRating,
            QualityRating = review.QualityRating,
            TeamworkRating = review.TeamworkRating,
            LeadershipRating = review.LeadershipRating,
            AverageRating = review.GetAverageRating(),
            Comments = review.Comments,
            StrengthAreas = review.StrengthAreas,
            ImprovementAreas = review.ImprovementAreas,
            Status = review.Status,
            IsFinal = review.IsFinal,
            Goals = review.Goals.Select(g => g.ToDto()).ToList(),
            Feedback = review.Feedback.Select(f => f.ToDto()).ToList()
        };
    }

    /// <summary>
    /// Maps PerformanceReview domain entity to PerformanceReviewDto (basic info).
    /// </summary>
    public static PerformanceReviewDto ToDto(this PerformanceReview review)
    {
        return new PerformanceReviewDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            ReviewerId = review.ReviewerId,
            EmployeeName = review.EmployeeName,
            ReviewerName = review.ReviewerName,
            ReviewYear = review.ReviewYear,
            ReviewQuarter = review.ReviewQuarter,
            ReviewDate = review.ReviewDate,
            DueDate = review.DueDate,
            CompletedDate = review.CompletedDate,
            Status = review.Status,
            IsFinal = review.IsFinal
        };
    }

    /// <summary>
    /// Maps PerformanceReview domain entity to PerformanceReviewListDto (list view).
    /// </summary>
    public static PerformanceReviewListDto ToListDto(this PerformanceReview review)
    {
        return new PerformanceReviewListDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            EmployeeName = review.EmployeeName,
            ReviewYear = review.ReviewYear,
            ReviewQuarter = review.ReviewQuarter,
            PerformanceRating = review.PerformanceRating,
            Status = review.Status,
            ReviewDate = review.ReviewDate,
            CompletedDate = review.CompletedDate
        };
    }
}
