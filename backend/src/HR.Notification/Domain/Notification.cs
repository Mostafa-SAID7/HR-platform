namespace HR.Notification.Domain;

/// <summary>
/// Notification aggregate root
/// </summary>
public class Notification : AggregateRoot
{
    public Guid RecipientId { get; private set; }
    public string RecipientEmail { get; private set; }
    public string RecipientPhone { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = [];

    private Notification() { }

    /// <summary>
    /// Create a new notification
    /// </summary>
    public static Notification Create(
        Guid recipientId,
        string recipientEmail,
        string recipientPhone,
        NotificationType type,
        NotificationChannel channel,
        string title,
        string content,
        Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Notification title is required");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Notification content is required");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = recipientId,
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone,
            Type = type,
            Channel = channel,
            Title = title,
            Content = content,
            Status = NotificationStatus.Pending,
            Metadata = metadata ?? [],
            CreatedAt = DateTime.UtcNow
        };

        notification.RaiseDomainEvent(new NotificationCreatedEvent(
            notification.Id,
            recipientId,
            type,
            channel,
            title));

        return notification;
    }

    /// <summary>
    /// Mark notification as sent
    /// </summary>
    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentEvent(Id, RecipientId));
    }

    /// <summary>
    /// Mark notification as delivered
    /// </summary>
    public void MarkAsDelivered()
    {
        if (Status == NotificationStatus.Sent)
        {
            Status = NotificationStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;

            RaiseDomainEvent(new NotificationDeliveredEvent(Id, RecipientId));
        }
    }

    /// <summary>
    /// Mark notification as read (for in-app notifications)
    /// </summary>
    public void MarkAsRead()
    {
        if (Status != NotificationStatus.Read)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;

            RaiseDomainEvent(new NotificationReadEvent(Id, RecipientId));
        }
    }

    /// <summary>
    /// Mark notification as failed
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new NotificationFailedEvent(Id, RecipientId, reason));
    }

    /// <summary>
    /// Increment retry count
    /// </summary>
    public void IncrementRetryCount()
    {
        RetryCount++;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if notification can be retried
    /// </summary>
    public bool CanRetry(int maxRetries = 3)
    {
        return Status == NotificationStatus.Failed && RetryCount < maxRetries;
    }
}

/// <summary>
/// Notification Template entity
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string TitleTemplate { get; set; } = null!; // Can include {{variable}} placeholders
    public string ContentTemplate { get; set; } = null!;
    public Dictionary<string, string> VariableMappings { get; set; } = []; // Variable name -> Field mapping
    public bool IsActive { get; set; } = true;

    public string RenderTitle(Dictionary<string, object> data)
    {
        var title = TitleTemplate;
        foreach (var kvp in data)
        {
            title = title.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
        }
        return title;
    }

    public string RenderContent(Dictionary<string, object> data)
    {
        var content = ContentTemplate;
        foreach (var kvp in data)
        {
            content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
        }
        return content;
    }
}

/// <summary>
/// Notification Preference entity
/// </summary>
public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public Dictionary<NotificationType, NotificationChannelPreference> TypePreferences { get; set; } = [];
    public DateTime? UnsubscribedAt { get; set; }

    public bool IsSubscribed => UnsubscribedAt == null;

    public bool CanReceive(NotificationType type, NotificationChannel channel)
    {
        if (!IsSubscribed)
            return false;

        return channel switch
        {
            NotificationChannel.Email => EmailEnabled,
            NotificationChannel.SMS => SmsEnabled,
            NotificationChannel.Push => PushEnabled,
            NotificationChannel.InApp => InAppEnabled,
            _ => false
        };
    }

    public void Unsubscribe()
    {
        UnsubscribedAt = DateTime.UtcNow;
    }

    public void Resubscribe()
    {
        UnsubscribedAt = null;
    }
}

/// <summary>
/// Notification Channel Preference
/// </summary>
public class NotificationChannelPreference
{
    public bool Enabled { get; set; } = true;
    public TimeSpan? QuietHoursStart { get; set; } // e.g., 22:00
    public TimeSpan? QuietHoursEnd { get; set; }   // e.g., 08:00
}

// Enums
public enum NotificationType
{
    EmployeeCreated = 0,
    PerformanceReviewDue = 1,
    LeaveApproved = 2,
    LeaveRejected = 3,
    PayslipGenerated = 4,
    InterviewScheduled = 5,
    OfferExtended = 6,
    ApplicationRejected = 7,
    SystemAlert = 8,
    Custom = 9
}

public enum NotificationChannel
{
    Email = 0,
    SMS = 1,
    Push = 2,
    InApp = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4,
    Cancelled = 5
}

// Domain Events
public record NotificationCreatedEvent(Guid Id, Guid RecipientId, NotificationType Type, NotificationChannel Channel, string Title) : IDomainEvent;
public record NotificationSentEvent(Guid Id, Guid RecipientId) : IDomainEvent;
public record NotificationDeliveredEvent(Guid Id, Guid RecipientId) : IDomainEvent;
public record NotificationReadEvent(Guid Id, Guid RecipientId) : IDomainEvent;
public record NotificationFailedEvent(Guid Id, Guid RecipientId, string Reason) : IDomainEvent;
