namespace HR.Performance.Application.Dtos.PerformanceFeedback.Mappings;

using HR.Performance.Domain;

/// <summary>
/// Centralized mapping configuration for PerformanceFeedback DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class PerformanceFeedbackDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceFeedback domain entity to PerformanceFeedbackDto.
    /// </summary>
    public static PerformanceFeedbackDto ToDto(this Feedback feedback)
    {
        return new PerformanceFeedbackDto
        {
            Id = feedback.Id,
            FeedbackProviderId = feedback.FeedbackProviderId,
            Comment = feedback.Comment,
            FeedbackRating = feedback.FeedbackRating,
            FeedbackCategory = feedback.FeedbackCategory,
            IsAnonymous = feedback.IsAnonymous,
            CreatedOnUtc = feedback.CreatedOnUtc
        };
    }

    /// <summary>
    /// Maps PerformanceFeedback domain entity to PerformanceFeedbackDetailDto.
    /// </summary>
    public static PerformanceFeedbackDetailDto ToDetailDto(this Feedback feedback, string providerName = "")
    {
        return new PerformanceFeedbackDetailDto
        {
            Id = feedback.Id,
            PerformanceReviewId = feedback.PerformanceReviewId,
            FeedbackProviderId = feedback.FeedbackProviderId,
            FeedbackProviderName = providerName,
            Comment = feedback.Comment,
            FeedbackRating = feedback.FeedbackRating,
            FeedbackCategory = feedback.FeedbackCategory,
            IsAnonymous = feedback.IsAnonymous,
            CreatedOnUtc = feedback.CreatedOnUtc
        };
    }
}
