namespace HR.Notification.Application.Mappings;

using HR.Notification.Domain;
using HR.Notification.Application.Dtos.Notification;

/// <summary>
/// Centralized mapping configuration for Notification DTOs.
/// </summary>
public static class NotificationDtoMappingConfiguration
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.RecipientId,
            notification.Type.ToString(),
            notification.Channel.ToString(),
            notification.Title,
            notification.Content,
            notification.Status.ToString(),
            notification.CreatedAt,
            notification.SentAt,
            notification.DeliveredAt,
            notification.ReadAt);
    }

    public static NotificationDetailDto ToDetailDto(this Notification notification)
    {
        return new NotificationDetailDto(
            notification.Id,
            notification.RecipientId,
            notification.RecipientEmail,
            notification.RecipientPhone,
            notification.Type.ToString(),
            notification.Channel.ToString(),
            notification.Title,
            notification.Content,
            notification.Status.ToString(),
            notification.CreatedAt,
            notification.SentAt,
            notification.DeliveredAt,
            notification.ReadAt,
            notification.FailureReason,
            notification.RetryCount);
    }

    public static NotificationListDto ToListDto(this Notification notification)
    {
        return new NotificationListDto(
            notification.Id,
            notification.RecipientId,
            notification.Title,
            notification.Status.ToString(),
            notification.CreatedAt);
    }
}
