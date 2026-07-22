namespace HR.Identity.Application.Options;

/// <summary>
/// SendGrid configuration for OTP email delivery
/// </summary>
public class SendGridOptions
{
    public const string SectionName = "Otp:SendGrid";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@hrplatform.com";
    public string FromName { get; set; } = "HR Platform";
}
