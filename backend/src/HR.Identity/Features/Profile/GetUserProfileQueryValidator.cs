namespace HR.Identity.Features.Profile;

using FluentValidation;

/// <summary>
/// Validator for GetUserProfileQuery
/// </summary>
public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");
    }
}
