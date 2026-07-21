namespace HR.Notification.Application.Services;

using Microsoft.Extensions.Logging;

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
