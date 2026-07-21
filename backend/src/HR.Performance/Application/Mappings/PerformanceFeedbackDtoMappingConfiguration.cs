namespace HR.Performance.Application.Mappings;

using HR.Performance.Domain;
using HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Mapping configuration for PerformanceFeedback DTOs.
/// </summary>
public static class PerformanceFeedbackDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceFeedback domain entity to PerformanceFeedbackDto.
    /// </summary>
    public static PerformanceFeedbackDto ToDto(this PerformanceFeedback feedback)
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
}
