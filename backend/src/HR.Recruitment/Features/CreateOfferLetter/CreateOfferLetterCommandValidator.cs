namespace HR.Recruitment.Features.CreateOfferLetter;

/// <summary>
/// Validator for CreateOfferLetterCommand
/// </summary>
public class CreateOfferLetterCommandValidator : AbstractValidator<CreateOfferLetterCommand>
{
    public CreateOfferLetterCommandValidator()
    {
        RuleFor(x => x.Request.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.Request.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required");

        RuleFor(x => x.Request.OfferSalary)
            .GreaterThan(0).WithMessage("Offer salary must be greater than 0");

        RuleFor(x => x.Request.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.Position)
            .NotEmpty().WithMessage("Position is required");

        RuleFor(x => x.Request.ProposedStartDate)
            .GreaterThan(DateTime.UtcNow.AddDays(1))
            .WithMessage("Proposed start date must be at least 2 days from now");
    }
}
