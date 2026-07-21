namespace HR.Performance.Features.SetPerformanceRatings;

using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Command to set performance ratings for a review.
/// </summary>
public record SetPerformanceRatingsCommand(Guid ReviewId, SetPerformanceRatingsRequest Request, Guid TenantId) : ICommand<PerformanceReviewDetailDto>;
