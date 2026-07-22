namespace HR.Identity.Application.Dtos.OAuth;

/// <summary>
/// DTO for OAuth provider information
/// </summary>
public record OAuthProviderDto(
    Guid Id,
    int ProviderType,
    string ProviderUserId,
    string ProviderEmail,
    string? ProviderName,
    string? ProfilePictureUrl,
    bool IsActive,
    DateTime ConnectedAt);
