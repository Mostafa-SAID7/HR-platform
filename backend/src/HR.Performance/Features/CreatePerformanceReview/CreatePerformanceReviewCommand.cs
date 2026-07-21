namespace HR.Performance.Features.CreatePerformanceReview;

using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Command to create a new performance review.
/// </summary>
public record CreatePerformanceReviewCommand(CreatePerformanceReviewRequest Request, Guid TenantId) : ICommand<PerformanceReviewDetailDto>;
