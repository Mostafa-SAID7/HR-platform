namespace HR.Notification.Features.SendNotification;

/// <summary>
/// Send a notification
/// </summary>
public record SendNotificationCommand(
    SendNotificationRequest Request,
    Guid TenantId) : ICommand<NotificationDto>;

/// <summary>
/// Validator for SendNotificationCommand
/// </summary>
public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.Request.RecipientId)
            .NotEmpty().WithMessage("Recipient ID is required");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Notification title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Notification content is required");

        RuleFor(x => x.Request.Channel)
            .Must(x => Enum.TryParse<NotificationChannel>(x, true, out _))
            .WithMessage("Invalid notification channel");

        RuleFor(x => x.Request.NotificationType)
            .Must(x => Enum.TryParse<NotificationType>(x, true, out _))
            .WithMessage("Invalid notification type");
    }
}

/// <summary>
/// Handler for SendNotificationCommand
/// </summary>
public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, NotificationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    public SendNotificationCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<Notification> notificationRepository,
        INotificationService notificationService,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<NotificationDto> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Parse channel and type
        if (!Enum.TryParse<NotificationChannel>(request.Request.Channel, true, out var channel))
            throw new DomainException($"Invalid channel: {request.Request.Channel}");

        if (!Enum.TryParse<NotificationType>(request.Request.NotificationType, true, out var type))
            throw new DomainException($"Invalid notification type: {request.Request.NotificationType}");

        // Create notification entity
        var notification = Notification.Create(
            request.Request.RecipientId,
            request.Request.RecipientEmail,
            request.Request.RecipientPhone,
            type,
            channel,
            request.Request.Title,
            request.Request.Content,
            request.Request.Metadata);

        // Add to repository
        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Attempt to send based on channel
        var recipient = channel switch
        {
            NotificationChannel.Email => request.Request.RecipientEmail,
            NotificationChannel.SMS => request.Request.RecipientPhone,
            NotificationChannel.Push => request.Request.RecipientId.ToString(),
            NotificationChannel.InApp => request.Request.RecipientId.ToString(),
            _ => throw new DomainException("Unknown notification channel")
        };

        try
        {
            var sendSuccess = await _notificationService.SendAsync(
                recipient,
                channel,
                request.Request.Title,
                request.Request.Content,
                cancellationToken);

            if (sendSuccess)
            {
                notification.MarkAsSent();
                _logger.LogInformation("Notification sent successfully via {Channel} to {Recipient}", channel, recipient);
            }
            else
            {
                notification.MarkAsFailed("Failed to send via channel service");
                _logger.LogWarning("Failed to send notification via {Channel} to {Recipient}", channel, recipient);
            }
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Error sending notification via {Channel} to {Recipient}", channel, recipient);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return notification.Adapt<NotificationDto>();
    }
}
