namespace HR.Recruitment.Features.GetJobPostings;

/// <summary>
/// Validator for GetJobPostingsQuery
/// </summary>
public class GetJobPostingsQueryValidator : AbstractValidator<GetJobPostingsQuery>
{
    public GetJobPostingsQueryValidator()
    {
        RuleFor(x => x.Filter.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.Filter.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
    }
}
