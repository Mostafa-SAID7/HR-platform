namespace HR.Notification.Application.Dtos.NotificationTemplate;

public record NotificationTemplateDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string TitleTemplate,
    string ContentTemplate,
    bool IsActive);
