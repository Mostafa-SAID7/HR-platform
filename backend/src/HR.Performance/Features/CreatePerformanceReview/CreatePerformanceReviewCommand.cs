namespace HR.Performance.Features.CreatePerformanceReview;

using HR.Performance.Application.Dtos;

/// <summary>
/// Command to create a new performance review.
/// </summary>
public record CreatePerformanceReviewCommand(CreatePerformanceReviewRequest Request, Guid TenantId) : ICommand<PerformanceReviewDetailDto>;

/// <summary>
/// Validator for CreatePerformanceReviewCommand.
/// </summary>
public class CreatePerformanceReviewCommandValidator : AbstractValidator<CreatePerformanceReviewCommand>
{
    public CreatePerformanceReviewCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.Request.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required");

        RuleFor(x => x.Request.ReviewYear)
            .GreaterThan(2000).WithMessage("Review year must be valid");

        RuleFor(x => x.Request.ReviewQuarter)
            .InclusiveBetween(1, 4).WithMessage("Review quarter must be between 1 and 4");

        RuleFor(x => x.Request.EmployeeName)
            .NotEmpty().WithMessage("Employee name is required")
            .MaximumLength(256).WithMessage("Employee name must not exceed 256 characters");

        RuleFor(x => x.Request.ReviewerName)
            .NotEmpty().WithMessage("Reviewer name is required")
            .MaximumLength(256).WithMessage("Reviewer name must not exceed 256 characters");
    }
}

/// <summary>
/// Handler for CreatePerformanceReviewCommand.
/// </summary>
public class CreatePerformanceReviewCommandHandler : IRequestHandler<CreatePerformanceReviewCommand, PerformanceReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePerformanceReviewCommandHandler> _logger;

    public CreatePerformanceReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreatePerformanceReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PerformanceReviewDetailDto> Handle(CreatePerformanceReviewCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Check if review already exists for this employee and period
        var reviewRepository = _unitOfWork.GetRepository<PerformanceReview>();
        var existingReview = await reviewRepository.GetAsQueryable()
            .FirstOrDefaultAsync(r => r.EmployeeId == req.EmployeeId &&
                r.ReviewYear == req.ReviewYear &&
                r.ReviewQuarter == req.ReviewQuarter &&
                r.TenantId == request.TenantId && !r.IsDeleted, cancellationToken);

        if (existingReview is not null)
        {
            _logger.LogWarning("Performance review already exists for employee {EmployeeId} in {Year}Q{Quarter}",
                req.EmployeeId, req.ReviewYear, req.ReviewQuarter);
            throw new AlreadyExistsException("PerformanceReview", $"{req.EmployeeId}-{req.ReviewYear}Q{req.ReviewQuarter}");
        }

        // Create new performance review
        var review = PerformanceReview.Create(
            employeeId: req.EmployeeId,
            reviewerId: req.ReviewerId,
            employeeName: req.EmployeeName,
            reviewerName: req.ReviewerName,
            reviewYear: req.ReviewYear,
            reviewQuarter: req.ReviewQuarter,
            tenantId: request.TenantId);

        await reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance review created for employee {EmployeeId} ({EmployeeName}) for {Year}Q{Quarter}",
            req.EmployeeId, req.EmployeeName, req.ReviewYear, req.ReviewQuarter);

        return MapToDetailDto(review);
    }

    private static PerformanceReviewDetailDto MapToDetailDto(PerformanceReview review)
    {
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
            Feedback = []
        };
    }
}
