namespace HR.Notification.Application.Dtos;

// Notification DTOs
public record SendNotificationRequest(
    Guid RecipientId,
    string RecipientEmail,
    string RecipientPhone,
    string NotificationType,
    string Channel,
    string Title,
    string Content,
    Dictionary<string, object>? Metadata = null);

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
    DateTime? ReadAt) : IMapFrom<Notification>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<Notification, NotificationDto>()
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.Channel, src => src.Channel.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());
}

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
    int RetryCount) : IMapFrom<Notification>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<Notification, NotificationDetailDto>()
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.Channel, src => src.Channel.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());
}

public record NotificationListDto(
    Guid Id,
    Guid RecipientId,
    string Title,
    string Status,
    DateTime CreatedAt);

public record NotificationFilterDto(
    Guid? RecipientId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10);

// Notification Template DTOs
public record CreateNotificationTemplateRequest(
    string Name,
    string Description,
    string NotificationType,
    string TitleTemplate,
    string ContentTemplate);

public record NotificationTemplateDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string TitleTemplate,
    string ContentTemplate,
    bool IsActive) : IMapFrom<NotificationTemplate>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<NotificationTemplate, NotificationTemplateDto>()
            .Map(dest => dest.Type, src => src.Type.ToString());
}

// Notification Preference DTOs
public record UpdateNotificationPreferenceRequest(
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool InAppEnabled);

public record NotificationPreferenceDto(
    Guid UserId,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool InAppEnabled,
    bool IsSubscribed) : IMapFrom<NotificationPreference>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<NotificationPreference, NotificationPreferenceDto>()
            .Map(dest => dest.IsSubscribed, src => src.IsSubscribed);
}

// Mark as read
public record MarkNotificationAsReadRequest;

// Batch send DTOs
public record BatchSendNotificationRequest(
    List<Guid> RecipientIds,
    string NotificationType,
    string Channel,
    string Title,
    string Content,
    Dictionary<string, object>? Metadata = null);

public record BatchSendResult(
    int TotalRecipients,
    int SuccessCount,
    int FailureCount,
    List<string> FailedRecipients);
