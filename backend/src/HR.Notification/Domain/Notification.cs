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
