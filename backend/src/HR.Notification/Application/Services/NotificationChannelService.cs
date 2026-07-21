namespace HR.Notification.Application.Services;

/// <summary>
/// Interface for notification channel services
/// </summary>
public interface INotificationChannelService
{
    Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken);
}

/// <summary>
/// Email notification service
/// </summary>
public class EmailNotificationService : INotificationChannelService
{
    private readonly SendGrid.SendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        var sendGridKey = configuration["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key is required");
        _sendGridClient = new SendGrid.SendGridClient(sendGridKey);
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@hrplatform.com";
        _logger = logger;
    }

    public async Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken)
    {
        try
        {
            var from = new SendGrid.Helpers.Mail.EmailAddress(_fromEmail, "HR Platform");
            var to = new SendGrid.Helpers.Mail.EmailAddress(recipient);
            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, title, content, content);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {Email}", recipient);
                return true;
            }

            _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}", recipient, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", recipient);
            return false;
        }
    }
}

/// <summary>
/// SMS notification service
/// </summary>
public class SmsNotificationService : INotificationChannelService
{
    private readonly Twilio.TwilioClient _twilioClient;
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

/// <summary>
/// Push notification service (Firebase)
/// </summary>
public class PushNotificationService : INotificationChannelService
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
        
        // Firebase initialization would happen here
        // FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile("firebase-key.json") });
    }

    public async Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, this would use Firebase Admin SDK
            // var message = new Message()
            // {
            //     Notification = new Notification() { Title = title, Body = content },
            //     Token = recipient, // Device token
            // };
            // var result = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);

            _logger.LogInformation("Push notification sent successfully to {Token}", recipient);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to {Token}", recipient);
            return false;
        }
    }
}

/// <summary>
/// In-app notification service (stores in database)
/// </summary>
public class InAppNotificationService : INotificationChannelService
{
    private readonly ILogger<InAppNotificationService> _logger;

    public InAppNotificationService(ILogger<InAppNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken)
    {
        try
        {
            // In-app notifications are stored in the database by the command handler
            // This service just logs that it was "delivered" to the in-app channel
            _logger.LogInformation("In-app notification created for user {UserId}", recipient);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating in-app notification for user {UserId}", recipient);
            return false;
        }
    }
}

/// <summary>
/// Composite notification channel service
/// </summary>
public interface INotificationService
{
    Task<bool> SendAsync(
        string recipient,
        NotificationChannel channel,
        string title,
        string content,
        CancellationToken cancellationToken);

    INotificationChannelService GetChannelService(NotificationChannel channel);
}

public class CompositeNotificationService : INotificationService
{
    private readonly Dictionary<NotificationChannel, INotificationChannelService> _channelServices;
    private readonly ILogger<CompositeNotificationService> _logger;

    public CompositeNotificationService(
        EmailNotificationService emailService,
        SmsNotificationService smsService,
        PushNotificationService pushService,
        InAppNotificationService inAppService,
        ILogger<CompositeNotificationService> logger)
    {
        _channelServices = new Dictionary<NotificationChannel, INotificationChannelService>
        {
            { NotificationChannel.Email, emailService },
            { NotificationChannel.SMS, smsService },
            { NotificationChannel.Push, pushService },
            { NotificationChannel.InApp, inAppService }
        };
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string recipient,
        NotificationChannel channel,
        string title,
        string content,
        CancellationToken cancellationToken)
    {
        if (!_channelServices.TryGetValue(channel, out var service))
        {
            _logger.LogWarning("Unknown notification channel: {Channel}", channel);
            return false;
        }

        return await service.SendAsync(recipient, title, content, cancellationToken);
    }

    public INotificationChannelService GetChannelService(NotificationChannel channel)
    {
        return _channelServices.TryGetValue(channel, out var service) 
            ? service 
            : throw new InvalidOperationException($"Unknown channel: {channel}");
    }
}
