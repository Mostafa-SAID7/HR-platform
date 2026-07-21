namespace HR.Performance.Features.AddPerformanceFeedback;

using HR.Performance.Application.Dtos.PerformanceFeedback;
using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Command to add feedback to a performance review.
/// </summary>
public record AddPerformanceFeedbackCommand(Guid ReviewId, AddFeedbackRequest Request, Guid TenantId) : ICommand<PerformanceReviewDetailDto>;
