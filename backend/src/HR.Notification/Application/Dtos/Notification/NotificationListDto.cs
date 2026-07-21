namespace HR.Notification.Application.Dtos.Notification;

public record NotificationListDto(
    Guid Id,
    Guid RecipientId,
    string Title,
    string Status,
    DateTime CreatedAt);
