namespace HR.Identity.Features.RefreshToken;

using HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// Command to refresh authentication token
/// </summary>
public record RefreshTokenCommand(
    RefreshTokenRequest Request,
    Guid TenantId) : ICommand<RefreshTokenResponse>;
