namespace HR.Performance.Features.GetPerformanceReviewById;

using HR.Performance.Application.Dtos.PerformanceReview;
using HR.Performance.Application.Mappings;
using HR.Performance.Domain;

/// <summary>
/// Handler for GetPerformanceReviewByIdQuery.
/// </summary>
public class GetPerformanceReviewByIdQueryHandler : IRequestHandler<GetPerformanceReviewByIdQuery, PerformanceReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPerformanceReviewByIdQueryHandler> _logger;

    public GetPerformanceReviewByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPerformanceReviewByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReviewDetailDto> Handle(GetPerformanceReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PerformanceReview>();
        var review = await repo.GetAsQueryable()
            .Include(r => r.Goals)
            .Include(r => r.Feedback)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.TenantId == request.TenantId, cancellationToken);

        if (review is null)
            throw new NotFoundException("PerformanceReview", request.ReviewId);

        return review.ToDetailDto();
    }
}
