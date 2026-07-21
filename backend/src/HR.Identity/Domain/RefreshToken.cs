namespace HR.Identity.Domain;

/// <summary>
/// Refresh token for token renewal.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDateUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDateUtc { get; set; }
    public User? User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDateUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Create a new refresh token.
    /// </summary>
    public static RefreshToken Create(Guid userId, Guid tenantId, int expiryDays = 7)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateToken(),
            ExpiryDateUtc = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revoke the refresh token.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedDateUtc = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
