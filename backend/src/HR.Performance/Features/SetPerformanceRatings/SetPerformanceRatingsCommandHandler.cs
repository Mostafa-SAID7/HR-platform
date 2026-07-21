namespace HR.Performance.Features.SetPerformanceRatings;

using HR.Performance.Application.Dtos.PerformanceReview;
using HR.Performance.Application.Mappings;
using HR.Performance.Domain;

/// <summary>
/// Handler for SetPerformanceRatingsCommand.
/// </summary>
public class SetPerformanceRatingsCommandHandler : IRequestHandler<SetPerformanceRatingsCommand, PerformanceReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetPerformanceRatingsCommandHandler> _logger;

    public SetPerformanceRatingsCommandHandler(IUnitOfWork unitOfWork, ILogger<SetPerformanceRatingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReviewDetailDto> Handle(SetPerformanceRatingsCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PerformanceReview>();
        var review = await repo.GetAsQueryable()
            .Include(r => r.Goals)
            .Include(r => r.Feedback)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.TenantId == request.TenantId, cancellationToken);
        
        if (review is null)
            throw new NotFoundException("PerformanceReview", request.ReviewId);

        review.SetRatings(
            request.Request.PerformanceRating,
            request.Request.ProductivityRating,
            request.Request.QualityRating,
            request.Request.TeamworkRating,
            request.Request.LeadershipRating);

        review.Comments = request.Request.Comments;
        review.StrengthAreas = request.Request.StrengthAreas;
        review.ImprovementAreas = request.Request.ImprovementAreas;

        repo.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ratings set for performance review {ReviewId}", request.ReviewId);

        return review.ToDetailDto();
    }
}
