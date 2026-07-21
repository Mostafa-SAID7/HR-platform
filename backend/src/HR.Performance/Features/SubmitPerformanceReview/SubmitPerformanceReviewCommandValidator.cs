namespace HR.Performance.Features.SubmitPerformanceReview;

/// <summary>
/// Validator for SubmitPerformanceReviewCommand.
/// </summary>
public class SubmitPerformanceReviewCommandValidator : AbstractValidator<SubmitPerformanceReviewCommand>
{
    public SubmitPerformanceReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");
    }
}
