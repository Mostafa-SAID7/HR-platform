namespace HR.Performance.Features.ApprovePerformanceReview;

using HR.Performance.Domain;

/// <summary>
/// Handler for ApprovePerformanceReviewCommand.
/// </summary>
public class ApprovePerformanceReviewCommandHandler : IRequestHandler<ApprovePerformanceReviewCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovePerformanceReviewCommandHandler> _logger;

    public ApprovePerformanceReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<ApprovePerformanceReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ApprovePerformanceReviewCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PerformanceReview>();
        var review = await repo.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
            throw new NotFoundException("PerformanceReview", request.ReviewId);

        review.Approve();
        repo.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance review {ReviewId} approved", request.ReviewId);
    }
}
