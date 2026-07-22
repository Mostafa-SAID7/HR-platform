namespace HR.Identity.Application.Dtos.ChangePassword;

/// <summary>
/// Change password response DTO
/// </summary>
public record ChangePasswordResponse(
    bool Success,
    string Message,
    DateTime ChangedAt);
