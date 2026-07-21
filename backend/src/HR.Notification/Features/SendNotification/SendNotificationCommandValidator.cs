namespace HR.Notification.Features.SendNotification;

using FluentValidation;

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
