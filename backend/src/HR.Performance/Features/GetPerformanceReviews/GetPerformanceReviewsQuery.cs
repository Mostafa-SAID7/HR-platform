namespace HR.Performance.Features.GetPerformanceReviews;

using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Query to get performance reviews with filtering and pagination.
/// </summary>
public record GetPerformanceReviewsQuery(PerformanceReviewFilterDto Filter, Guid TenantId) : IQuery<PaginatedResult<PerformanceReviewListDto>>;
