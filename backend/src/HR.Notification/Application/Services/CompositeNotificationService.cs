namespace HR.Notification.Application.Services;

using Microsoft.Extensions.Logging;

/// <summary>
/// Composite notification channel service - routes to appropriate channel
/// </summary>
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
