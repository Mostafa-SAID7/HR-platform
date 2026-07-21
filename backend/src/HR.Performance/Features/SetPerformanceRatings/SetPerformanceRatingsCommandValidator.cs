namespace HR.Performance.Features.SetPerformanceRatings;

using HR.Performance.Application.Dtos.PerformanceReview;

/// <summary>
/// Validator for SetPerformanceRatingsCommand.
/// </summary>
public class SetPerformanceRatingsCommandValidator : AbstractValidator<SetPerformanceRatingsCommand>
{
    public SetPerformanceRatingsCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");

        RuleFor(x => x.Request.PerformanceRating)
            .GreaterThanOrEqualTo(0).WithMessage("Performance rating must be non-negative")
            .LessThanOrEqualTo(5).WithMessage("Performance rating must not exceed 5");

        RuleFor(x => x.Request.ProductivityRating)
            .GreaterThanOrEqualTo(0).WithMessage("Productivity rating must be non-negative")
            .LessThanOrEqualTo(5).WithMessage("Productivity rating must not exceed 5");

        RuleFor(x => x.Request.QualityRating)
            .GreaterThanOrEqualTo(0).WithMessage("Quality rating must be non-negative")
            .LessThanOrEqualTo(5).WithMessage("Quality rating must not exceed 5");

        RuleFor(x => x.Request.TeamworkRating)
            .GreaterThanOrEqualTo(0).WithMessage("Teamwork rating must be non-negative")
            .LessThanOrEqualTo(5).WithMessage("Teamwork rating must not exceed 5");

        RuleFor(x => x.Request.LeadershipRating)
            .GreaterThanOrEqualTo(0).WithMessage("Leadership rating must be non-negative")
            .LessThanOrEqualTo(5).WithMessage("Leadership rating must not exceed 5");
    }
}
