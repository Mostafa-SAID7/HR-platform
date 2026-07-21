namespace HR.Performance.Features.SubmitPerformanceReview;

/// <summary>
/// Command to submit a performance review.
/// </summary>
public record SubmitPerformanceReviewCommand(Guid ReviewId, Guid TenantId) : ICommand;
