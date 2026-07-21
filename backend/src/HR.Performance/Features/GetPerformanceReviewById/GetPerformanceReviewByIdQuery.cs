namespace HR.Performance.Features.GetPerformanceReviewById;

using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Query to get a performance review by ID.
/// </summary>
public record GetPerformanceReviewByIdQuery(Guid ReviewId, Guid TenantId) : IQuery<PerformanceReviewDetailDto>;
