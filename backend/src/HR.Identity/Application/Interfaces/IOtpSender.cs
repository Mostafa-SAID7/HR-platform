namespace HR.Identity.Application.Interfaces;

/// <summary>
/// Interface for OTP delivery services (email via SendGrid)
/// </summary>
public interface IOtpSender
{
    /// <summary>
    /// Send OTP via email
    /// </summary>
    Task<bool> SendOtpEmailAsync(string email, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default);
}
