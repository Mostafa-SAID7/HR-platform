namespace HR.Recruitment.Features.ApplyJob;

/// <summary>
/// Validator for ApplyJobCommand
/// </summary>
public class ApplyJobCommandValidator : AbstractValidator<ApplyJobCommand>
{
    public ApplyJobCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty().WithMessage("Job posting ID is required");

        RuleFor(x => x.Request.CandidateName)
            .NotEmpty().WithMessage("Candidate name is required")
            .MaximumLength(200).WithMessage("Candidate name must not exceed 200 characters");

        RuleFor(x => x.Request.CandidateEmail)
            .NotEmpty().WithMessage("Candidate email is required")
            .EmailAddress().WithMessage("Valid email is required");

        RuleFor(x => x.Request.CandidatePhone)
            .NotEmpty().WithMessage("Candidate phone is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Request.Resume)
            .NotEmpty().WithMessage("Resume is required");
    }
}
