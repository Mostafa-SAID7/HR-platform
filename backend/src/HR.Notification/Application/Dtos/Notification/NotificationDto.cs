namespace HR.Notification.Application.Dtos.Notification;

public record NotificationDto(
    Guid Id,
    Guid RecipientId,
    string Type,
    string Channel,
    string Title,
    string Content,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt);
