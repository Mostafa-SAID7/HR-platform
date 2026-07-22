namespace HR.Identity.Application.Dtos.ChangePassword;

/// <summary>
/// Change password request DTO
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
