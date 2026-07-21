namespace HR.Notification.Application.Dtos.NotificationPreference;

public record NotificationPreferenceDto(
    Guid UserId,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool InAppEnabled,
    bool IsSubscribed);
