namespace HR.Identity.Features.RequestOtp;

using FluentValidation;

/// <summary>
/// Validator for RequestOtpCommand
/// </summary>
public class RequestOtpValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.OtpType)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(4)
            .WithMessage("Invalid OTP type");

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Invalid phone number format");
        });
    }
}
