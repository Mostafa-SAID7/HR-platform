namespace HR.Notification.Application.Dtos.NotificationPreference;

public record UpdateNotificationPreferenceRequest(
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool InAppEnabled);
