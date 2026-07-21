namespace HR.Notification.Application.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// SMS notification service using Twilio
/// </summary>
public class SmsNotificationService : INotificationChannelService
{
    private readonly string _fromPhoneNumber;
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(IConfiguration configuration, ILogger<SmsNotificationService> logger)
    {
        var accountSid = configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio Account SID is required");
        var authToken = configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio Auth Token is required");
        _fromPhoneNumber = configuration["Twilio:FromPhoneNumber"] ?? throw new InvalidOperationException("Twilio phone number is required");

        Twilio.TwilioClient.Init(accountSid, authToken);
        _logger = logger;
    }

    public async Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken)
    {
        try
        {
            // Combine title and content for SMS
            var message = $"{title}: {content}".Substring(0, Math.Min(160, $"{title}: {content}".Length));

            var result = await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(recipient));

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. SID: {MessageSid}", recipient, result.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", recipient);
            return false;
        }
    }
}
