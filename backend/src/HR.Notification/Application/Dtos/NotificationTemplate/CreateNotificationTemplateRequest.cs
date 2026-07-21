namespace HR.Notification.Application.Dtos.NotificationTemplate;

public record CreateNotificationTemplateRequest(
    string Name,
    string Description,
    string NotificationType,
    string TitleTemplate,
    string ContentTemplate);
