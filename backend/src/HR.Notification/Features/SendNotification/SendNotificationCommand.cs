namespace HR.Notification.Features.SendNotification;

using HR.Notification.Application.Dtos.Notification;

/// <summary>
/// Send a notification
/// </summary>
public record SendNotificationCommand(
    SendNotificationRequest Request,
    Guid TenantId) : ICommand<NotificationDto>;
