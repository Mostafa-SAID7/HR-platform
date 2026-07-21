namespace HR.Notification.Application.Dtos.Notification;

public record NotificationDetailDto(
    Guid Id,
    Guid RecipientId,
    string RecipientEmail,
    string RecipientPhone,
    string Type,
    string Channel,
    string Title,
    string Content,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt,
    string? FailureReason,
    int RetryCount);
