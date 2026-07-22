namespace HR.Identity.Features.ChangePassword;

using FluentValidation;
using HR.Identity.Application.Dtos.UserProfile;

/// <summary>
/// Validator for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("User ID is required");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Change password request cannot be null");

        RuleFor(x => x.Request.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("New password must contain at least one digit")
            .Matches(@"[^A-Za-z0-9]")
            .WithMessage("New password must contain at least one special character");

        RuleFor(x => x.Request.ConfirmPassword)
            .Equal(x => x.Request.NewPassword)
            .WithMessage("Passwords do not match");

        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tenant ID is required");
    }
}
