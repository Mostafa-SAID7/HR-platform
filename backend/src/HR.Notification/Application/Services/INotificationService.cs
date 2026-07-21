namespace HR.Notification.Application.Services;

/// <summary>
/// Composite notification service interface
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
