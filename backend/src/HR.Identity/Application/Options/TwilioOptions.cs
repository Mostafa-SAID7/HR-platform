namespace HR.Identity.Application.Options;

/// <summary>
/// Twilio configuration (optional SMS support - not currently used)
/// </summary>
public class TwilioOptions
{
    public const string SectionName = "Otp:Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
}
