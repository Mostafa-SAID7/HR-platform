namespace HR.Notification.Application.Dtos.Notification;

public record SendNotificationRequest(
    Guid RecipientId,
    string RecipientEmail,
    string RecipientPhone,
    string NotificationType,
    string Channel,
    string Title,
    string Content,
    Dictionary<string, object>? Metadata = null);
