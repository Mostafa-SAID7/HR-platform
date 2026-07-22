namespace HR.Identity.Application.Services;

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;

/// <summary>
/// Twilio-based OTP sender implementation for SMS
/// Note: Currently not used - only SendGrid for email OTP
/// </summary>
public class TwilioOtpSender : IOtpSender
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioOtpSender> _logger;

    public TwilioOtpSender(IOptions<TwilioOptions> options, ILogger<TwilioOtpSender> logger)
    {
        _options = options.Value;
        if (!string.IsNullOrEmpty(_options.AccountSid) && !string.IsNullOrEmpty(_options.AuthToken))
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        }
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default)
    {
        // Twilio SendGrid integration can be used, but primary use is SMS
        _logger.LogWarning("Twilio OTP sender is primarily for SMS. Use SendGrid provider for email.");
        return await Task.FromResult(false);
    }

    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = $"Your HR Platform OTP is: {otpCode}. Valid for {expiryMinutes} minutes. Do not share this code.";

            var smsMessage = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_options.FromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            _logger.LogInformation("OTP SMS sent successfully to {PhoneNumber}. SID: {MessageSid}", phoneNumber, smsMessage.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
