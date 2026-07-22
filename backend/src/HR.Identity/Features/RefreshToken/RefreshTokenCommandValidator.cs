namespace HR.Identity.Features.RefreshToken;

using FluentValidation;
using HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Refresh token request cannot be null");

        RuleFor(x => x.Request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");

        RuleFor(x => x.Request.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tenant ID is required");
    }
}
