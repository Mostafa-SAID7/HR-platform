namespace HR.Identity.Features.ChangePassword;

using HR.Identity.Application.Dtos.UserProfile;

/// <summary>
/// Command to change user password
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    ChangePasswordRequest Request,
    Guid TenantId) : ICommand<ChangePasswordResponse>;
