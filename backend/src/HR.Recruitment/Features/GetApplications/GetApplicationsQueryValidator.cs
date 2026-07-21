namespace HR.Recruitment.Features.GetApplications;

/// <summary>
/// Validator for GetApplicationsQuery
/// </summary>
public class GetApplicationsQueryValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsQueryValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty().WithMessage("Job posting ID is required");
    }
}
