namespace HR.Notification.Domain;

/// <summary>
/// NotificationPreference aggregate root
/// </summary>
public class NotificationPreference : AggregateRoot
{
    public Guid UserId { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool SmsEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool IsSubscribed { get; private set; }

    private NotificationPreference() { }

    /// <summary>
    /// Create new notification preference for user
    /// </summary>
    public static NotificationPreference Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required");

        var preference = new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true,
            PushEnabled = true,
            InAppEnabled = true,
            IsSubscribed = true,
            CreatedAt = DateTime.UtcNow
        };

        preference.RaiseDomainEvent(new NotificationPreferenceCreatedEvent(preference.Id, userId));

        return preference;
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    public void Update(bool emailEnabled, bool smsEnabled, bool pushEnabled, bool inAppEnabled)
    {
        EmailEnabled = emailEnabled;
        SmsEnabled = smsEnabled;
        PushEnabled = pushEnabled;
        InAppEnabled = inAppEnabled;
        IsSubscribed = emailEnabled || smsEnabled || pushEnabled || inAppEnabled;

        RaiseDomainEvent(new NotificationPreferenceUpdatedEvent(Id, UserId));
    }

    /// <summary>
    /// Unsubscribe user from all notifications
    /// </summary>
    public void Unsubscribe()
    {
        IsSubscribed = false;
        EmailEnabled = false;
        SmsEnabled = false;
        PushEnabled = false;
        InAppEnabled = false;

        RaiseDomainEvent(new NotificationPreferenceUnsubscribedEvent(Id, UserId));
    }
}
