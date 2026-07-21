namespace HR.Recruitment.Features.CreateJobPosting;

/// <summary>
/// Validator for CreateJobPostingCommand
/// </summary>
public class CreateJobPostingCommandValidator : AbstractValidator<CreateJobPostingCommand>
{
    public CreateJobPostingCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Job description is required")
            .MinimumLength(50).WithMessage("Job description must be at least 50 characters");

        RuleFor(x => x.Request.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.RequiredSkills)
            .NotEmpty().WithMessage("At least one required skill must be specified");

        RuleFor(x => x.Request.SalaryMin)
            .GreaterThan(0).When(x => x.Request.SalaryMin.HasValue).WithMessage("Minimum salary must be greater than 0");

        RuleFor(x => x.Request.SalaryMax)
            .GreaterThan(x => x.Request.SalaryMin).When(x => x.Request.SalaryMin.HasValue && x.Request.SalaryMax.HasValue)
            .WithMessage("Maximum salary must be greater than minimum salary");
    }
}
