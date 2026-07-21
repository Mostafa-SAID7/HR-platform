namespace HR.Performance.Features.ApprovePerformanceReview;

/// <summary>
/// Command to approve a performance review.
/// </summary>
public record ApprovePerformanceReviewCommand(Guid ReviewId, Guid TenantId) : ICommand;
