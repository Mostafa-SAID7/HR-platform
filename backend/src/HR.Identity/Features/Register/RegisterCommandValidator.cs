namespace HR.Identity.Features.Register;

using FluentValidation;
using HR.Identity.Application.Dtos.Register;

/// <summary>
/// Validator for RegisterCommand
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Register request cannot be null");

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be valid");

        RuleFor(x => x.Request.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters");

        RuleFor(x => x.Request.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(100)
            .WithMessage("Full name cannot exceed 100 characters");

        RuleFor(x => x.Request.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches(@"[^A-Za-z0-9]")
            .WithMessage("Password must contain at least one special character");

        RuleFor(x => x.Request.ConfirmPassword)
            .Equal(x => x.Request.Password)
            .WithMessage("Passwords do not match");

        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tenant ID is required");
    }
}
