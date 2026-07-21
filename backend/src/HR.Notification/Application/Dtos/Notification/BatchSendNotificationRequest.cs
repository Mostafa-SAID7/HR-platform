namespace HR.Notification.Application.Dtos.Notification;

public record BatchSendNotificationRequest(
    List<Guid> RecipientIds,
    string NotificationType,
    string Channel,
    string Title,
    string Content,
    Dictionary<string, object>? Metadata = null);
