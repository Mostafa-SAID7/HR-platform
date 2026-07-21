namespace HR.Notification.Domain;

/// <summary>
/// NotificationTemplate aggregate root
/// </summary>
public class NotificationTemplate : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public NotificationType Type { get; private set; }
    public string TitleTemplate { get; private set; }
    public string ContentTemplate { get; private set; }
    public Dictionary<string, string> VariableMappings { get; private set; } = [];
    public bool IsActive { get; private set; }

    private NotificationTemplate() { }

    /// <summary>
    /// Create a new notification template
    /// </summary>
    public static NotificationTemplate Create(
        string name,
        string description,
        NotificationType type,
        string titleTemplate,
        string contentTemplate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required");
        if (string.IsNullOrWhiteSpace(titleTemplate))
            throw new DomainException("Title template is required");
        if (string.IsNullOrWhiteSpace(contentTemplate))
            throw new DomainException("Content template is required");

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            TitleTemplate = titleTemplate,
            ContentTemplate = contentTemplate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        template.RaiseDomainEvent(new NotificationTemplateCreatedEvent(template.Id, name, type));

        return template;
    }

    /// <summary>
    /// Deactivate the template
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        RaiseDomainEvent(new NotificationTemplateDeactivatedEvent(Id));
    }

    /// <summary>
    /// Activate the template
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        RaiseDomainEvent(new NotificationTemplateActivatedEvent(Id));
    }
}
