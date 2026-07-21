namespace HR.Performance.Features.SubmitPerformanceReview;

public record SubmitPerformanceReviewCommand(Guid ReviewId, Guid TenantId) : ICommand;

public class SubmitPerformanceReviewCommandHandler : IRequestHandler<SubmitPerformanceReviewCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubmitPerformanceReviewCommandHandler> _logger;

    public SubmitPerformanceReviewCommandHandler(IUnitOfWork unitOfWork, ILogger<SubmitPerformanceReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(SubmitPerformanceReviewCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<PerformanceReview>();
        var review = await repo.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
            throw new NotFoundException("PerformanceReview", request.ReviewId);

        review.Submit();
        repo.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance review {ReviewId} submitted", request.ReviewId);
    }
}
