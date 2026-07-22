namespace HR.Identity.Application.Options;

/// <summary>
/// OTP (One-Time Password) configuration options
/// Email delivery via SendGrid
/// </summary>
public class OtpOptions
{
    public const string SectionName = "Otp";

    public bool Enabled { get; set; } = true;
    public int ExpiryMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 3;
    public int[] AllowedTypes { get; set; } = [ 0, 1, 2 ]; // EmailVerification, PhoneVerification, PasswordReset
}

