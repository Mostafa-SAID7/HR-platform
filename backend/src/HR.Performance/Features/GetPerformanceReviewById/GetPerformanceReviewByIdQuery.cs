namespace HR.Performance.Features.GetPerformanceReviewById;

using HR.Performance.Application.Dtos;

public record GetPerformanceReviewByIdQuery(Guid ReviewId, Guid TenantId) : IQuery<PerformanceReviewDetailDto>;

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

        return new PerformanceReviewDetailDto
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            ReviewerId = review.ReviewerId,
            EmployeeName = review.EmployeeName,
            ReviewerName = review.ReviewerName,
            ReviewYear = review.ReviewYear,
            ReviewQuarter = review.ReviewQuarter,
            ReviewDate = review.ReviewDate,
            DueDate = review.DueDate,
            CompletedDate = review.CompletedDate,
            PerformanceRating = review.PerformanceRating,
            ProductivityRating = review.ProductivityRating,
            QualityRating = review.QualityRating,
            TeamworkRating = review.TeamworkRating,
            LeadershipRating = review.LeadershipRating,
            AverageRating = review.GetAverageRating(),
            Comments = review.Comments,
            StrengthAreas = review.StrengthAreas,
            ImprovementAreas = review.ImprovementAreas,
            Status = review.Status,
            IsFinal = review.IsFinal,
            Goals = review.Goals.Select(g => new PerformanceGoalDto
            {
                Id = g.Id,
                GoalTitle = g.GoalTitle,
                GoalDescription = g.GoalDescription,
                TargetValue = g.TargetValue,
                ActualValue = g.ActualValue,
                UnitOfMeasure = g.UnitOfMeasure,
                StartDate = g.StartDate,
                DueDate = g.DueDate,
                Status = g.Status,
                Weight = g.Weight,
                CompletionPercentage = g.GetCompletionPercentage()
            }).ToList(),
            Feedback = review.Feedback.Select(f => new PerformanceFeedbackDto
            {
                Id = f.Id,
                FeedbackProviderId = f.FeedbackProviderId,
                Comment = f.Comment,
                FeedbackRating = f.FeedbackRating,
                FeedbackCategory = f.FeedbackCategory,
                IsAnonymous = f.IsAnonymous,
                CreatedOnUtc = f.CreatedOnUtc
            }).ToList()
        };
    }
}
