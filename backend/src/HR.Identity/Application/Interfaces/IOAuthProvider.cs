namespace HR.Identity.Application.Interfaces;

/// <summary>
/// OAuth provider interface for token validation and user info extraction
/// Supports: Google, Facebook
/// </summary>
public interface IOAuthProvider
{
    /// <summary>
    /// Validate access token with OAuth provider
    /// </summary>
    Task<OAuthUserInfo?> ValidateAndGetUserAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate ID token and extract claims
    /// </summary>
    Task<OAuthUserInfo?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider name
    /// </summary>
    string ProviderName { get; }
}

/// <summary>
/// OAuth user information extracted from provider
/// </summary>
public record OAuthUserInfo(
    string ProviderId,
    string Email,
    string Name,
    string? ProfilePictureUrl,
    Dictionary<string, string> AdditionalClaims);
