namespace HR.Performance.Features.AddPerformanceFeedback;

using HR.Performance.Application.Dtos.PerformanceFeedback;
using HR.Performance.Application.Dtos.PerformanceReview;
using HR.Performance.Application.Mappings;
using HR.Performance.Domain;

/// <summary>
/// Handler for AddPerformanceFeedbackCommand.
/// </summary>
public class AddPerformanceFeedbackCommandHandler : IRequestHandler<AddPerformanceFeedbackCommand, PerformanceReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddPerformanceFeedbackCommandHandler> _logger;

    public AddPerformanceFeedbackCommandHandler(IUnitOfWork unitOfWork, ILogger<AddPerformanceFeedbackCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReviewDetailDto> Handle(AddPerformanceFeedbackCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PerformanceReview>();
        var review = await repo.GetAsQueryable()
            .Include(r => r.Goals)
            .Include(r => r.Feedback)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.TenantId == request.TenantId, cancellationToken);
        
        if (review is null)
            throw new NotFoundException("PerformanceReview", request.ReviewId);

        review.AddFeedback(request.Request.FeedbackProviderId, request.Request.Comment);

        repo.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feedback added to performance review {ReviewId}", request.ReviewId);

        return review.ToDetailDto();
    }
}
