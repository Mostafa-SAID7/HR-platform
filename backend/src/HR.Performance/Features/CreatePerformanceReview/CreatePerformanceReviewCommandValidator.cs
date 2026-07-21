namespace HR.Performance.Features.CreatePerformanceReview;

using HR.Performance.Application.Dtos.PerformanceReview;

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
