namespace HR.Performance.Features.GetPerformanceReviews;

using HR.Performance.Application.Dtos;

/// <summary>
/// Query to get performance reviews with filtering and pagination.
/// </summary>
public record GetPerformanceReviewsQuery(PerformanceFilterDto Filter, Guid TenantId) : IQuery<PaginatedResult<PerformanceReviewListDto>>;

/// <summary>
/// Handler for GetPerformanceReviewsQuery.
/// </summary>
public class GetPerformanceReviewsQueryHandler : IRequestHandler<GetPerformanceReviewsQuery, PaginatedResult<PerformanceReviewListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPerformanceReviewsQueryHandler> _logger;

    public GetPerformanceReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPerformanceReviewsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<PerformanceReviewListDto>> Handle(GetPerformanceReviewsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        filter.Validate();

        var skip = PaginationHelper.CalculateSkip(filter.PageNumber, filter.PageSize);

        var query = _unitOfWork.GetRepository<PerformanceReview>().GetAsQueryable();

        // Apply filters
        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == filter.EmployeeId);
        }

        if (filter.ReviewYear.HasValue)
        {
            query = query.Where(r => r.ReviewYear == filter.ReviewYear);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(r => r.Status == filter.Status);
        }

        if (filter.MinRating.HasValue)
        {
            query = query.Where(r => r.PerformanceRating >= filter.MinRating);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(r => r.EmployeeName.Contains(filter.SearchTerm) || r.ReviewerName.Contains(filter.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.ReviewDate)
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(r => new PerformanceReviewListDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.EmployeeName,
                ReviewYear = r.ReviewYear,
                ReviewQuarter = r.ReviewQuarter,
                PerformanceRating = r.PerformanceRating,
                Status = r.Status,
                ReviewDate = r.ReviewDate,
                CompletedDate = r.CompletedDate
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} performance reviews for tenant {TenantId}", items.Count, request.TenantId);

        return PaginatedResult<PerformanceReviewListDto>.Create(items, filter.PageNumber, filter.PageSize, totalCount);
    }
}
