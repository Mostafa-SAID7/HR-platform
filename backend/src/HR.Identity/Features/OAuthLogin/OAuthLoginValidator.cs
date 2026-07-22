namespace HR.Identity.Features.OAuthLogin;

using FluentValidation;

/// <summary>
/// Validator for OAuthLoginCommand
/// </summary>
public class OAuthLoginValidator : AbstractValidator<OAuthLoginCommand>
{
    public OAuthLoginValidator()
    {
        RuleFor(x => x.ProviderType)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(4)
            .WithMessage("Invalid OAuth provider type");

        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required")
            .MinimumLength(10)
            .WithMessage("Access token appears invalid");
    }
}
