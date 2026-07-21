namespace HR.Notification.Application.Services;

/// <summary>
/// Interface for notification channel services
/// </summary>
public interface INotificationChannelService
{
    Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken);
}
