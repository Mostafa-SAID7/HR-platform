namespace HR.Notification.Application.Dtos.Notification;

public record NotificationFilterDto(
    Guid? RecipientId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10);
