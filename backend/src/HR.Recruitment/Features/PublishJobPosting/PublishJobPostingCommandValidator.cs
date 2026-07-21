namespace HR.Recruitment.Features.PublishJobPosting;

/// <summary>
/// Validator for PublishJobPostingCommand
/// </summary>
public class PublishJobPostingCommandValidator : AbstractValidator<PublishJobPostingCommand>
{
    public PublishJobPostingCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty().WithMessage("Job posting ID is required");
    }
}
