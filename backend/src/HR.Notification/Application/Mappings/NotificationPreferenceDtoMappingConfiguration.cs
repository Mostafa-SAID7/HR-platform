namespace HR.Notification.Application.Mappings;

using HR.Notification.Domain;
using HR.Notification.Application.Dtos.NotificationPreference;

/// <summary>
/// Centralized mapping configuration for NotificationPreference DTOs.
/// </summary>
public static class NotificationPreferenceDtoMappingConfiguration
{
    public static NotificationPreferenceDto ToDto(this NotificationPreference preference)
    {
        return new NotificationPreferenceDto(
            preference.UserId,
            preference.EmailEnabled,
            preference.SmsEnabled,
            preference.PushEnabled,
            preference.InAppEnabled,
            preference.IsSubscribed);
    }
}
