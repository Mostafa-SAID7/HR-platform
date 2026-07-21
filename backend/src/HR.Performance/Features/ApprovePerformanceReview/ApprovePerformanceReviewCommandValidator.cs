namespace HR.Performance.Features.ApprovePerformanceReview;

/// <summary>
/// Validator for ApprovePerformanceReviewCommand.
/// </summary>
public class ApprovePerformanceReviewCommandValidator : AbstractValidator<ApprovePerformanceReviewCommand>
{
    public ApprovePerformanceReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");
    }
}
