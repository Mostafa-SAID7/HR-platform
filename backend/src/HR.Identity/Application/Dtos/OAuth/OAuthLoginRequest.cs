namespace HR.Identity.Application.Dtos.OAuth;

/// <summary>
/// Request DTO for OAuth login
/// </summary>
public record OAuthLoginRequest
{
    public int ProviderType { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }

    public OAuthLoginRequest(int providerType, string accessToken, string? refreshToken = null, string? idToken = null)
    {
        ProviderType = providerType;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IdToken = idToken;
    }
}
