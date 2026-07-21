namespace HR.Notification.Application.Dtos.Notification;

public record BatchSendResult(
    int TotalRecipients,
    int SuccessCount,
    int FailureCount,
    List<string> FailedRecipients);
