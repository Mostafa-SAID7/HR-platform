namespace HR.Notification.Application.Mappings;

using HR.Notification.Domain;
using HR.Notification.Application.Dtos.NotificationTemplate;

/// <summary>
/// Centralized mapping configuration for NotificationTemplate DTOs.
/// </summary>
public static class NotificationTemplateDtoMappingConfiguration
{
    public static NotificationTemplateDto ToDto(this NotificationTemplate template)
    {
        return new NotificationTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            template.Type.ToString(),
            template.TitleTemplate,
            template.ContentTemplate,
            template.IsActive);
    }
}
