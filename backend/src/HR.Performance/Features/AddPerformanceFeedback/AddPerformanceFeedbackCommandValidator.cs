namespace HR.Performance.Features.AddPerformanceFeedback;

using HR.Performance.Application.Dtos.PerformanceFeedback;

/// <summary>
/// Validator for AddPerformanceFeedbackCommand.
/// </summary>
public class AddPerformanceFeedbackCommandValidator : AbstractValidator<AddPerformanceFeedbackCommand>
{
    public AddPerformanceFeedbackCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");

        RuleFor(x => x.Request.FeedbackProviderId)
            .NotEmpty().WithMessage("Feedback provider ID is required");

        RuleFor(x => x.Request.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters");

        RuleFor(x => x.Request.FeedbackRating)
            .GreaterThanOrEqualTo(1).WithMessage("Feedback rating must be at least 1")
            .LessThanOrEqualTo(5).WithMessage("Feedback rating must not exceed 5");
    }
}
