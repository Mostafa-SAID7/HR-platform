namespace HR.Identity.Domain;

/// <summary>
/// OAuth provider configuration and credentials for social login
/// </summary>
public class OAuthProvider : BaseEntity
{
    public Guid UserId { get; private set; }
    public OAuthProviderType ProviderType { get; private set; }
    public string ProviderUserId { get; private set; } = string.Empty;
    public string ProviderEmail { get; private set; } = string.Empty;
    public string? ProviderName { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? TokenExpiry { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime ConnectedAt { get; private set; }
    public User? User { get; private set; }

    private OAuthProvider() { }

    /// <summary>
    /// Create new OAuth provider connection
    /// </summary>
    public static OAuthProvider Create(
        Guid userId,
        OAuthProviderType providerType,
        string providerUserId,
        string providerEmail,
        string? providerName,
        string? profilePictureUrl,
        Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(providerUserId))
            throw new DomainException("Provider user ID is required");

        if (string.IsNullOrWhiteSpace(providerEmail))
            throw new DomainException("Provider email is required");

        var oauthProvider = new OAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderType = providerType,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
            ProviderName = providerName,
            ProfilePictureUrl = profilePictureUrl,
            IsActive = true,
            ConnectedAt = DateTime.UtcNow,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        return oauthProvider;
    }

    /// <summary>
    /// Update OAuth tokens
    /// </summary>
    public void UpdateTokens(string accessToken, string? refreshToken = null, int expirySeconds = 3600)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiry = DateTime.UtcNow.AddSeconds(expirySeconds);
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if access token is expired
    /// </summary>
    public bool IsTokenExpired => TokenExpiry.HasValue && DateTime.UtcNow >= TokenExpiry;

    /// <summary>
    /// Disconnect OAuth provider
    /// </summary>
    public void Disconnect()
    {
        IsActive = false;
        AccessToken = null;
        RefreshToken = null;
        TokenExpiry = null;
        LastModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// OAuth provider type enumeration
/// </summary>
public enum OAuthProviderType
{
    Google = 0,
    Facebook = 1,
    GitHub = 2,
    Microsoft = 3,
    LinkedIn = 4
}
