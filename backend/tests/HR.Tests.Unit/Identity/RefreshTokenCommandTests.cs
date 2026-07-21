namespace HR.Tests.Unit.Identity;

/// <summary>
/// Unit tests for RefreshToken domain entity.
/// Tests cover: token creation, expiration, revocation, and validation.
/// </summary>
public class RefreshTokenCommandTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var expiryDays = 7;

        // Act
        var refreshToken = RefreshToken.Create(userId, tenantId, expiryDays);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(tenantId, refreshToken.TenantId);
        Assert.NotEmpty(refreshToken.Token);
        Assert.False(refreshToken.IsRevoked);
        Assert.Null(refreshToken.RevokedDateUtc);
        Assert.False(refreshToken.IsExpired);
        Assert.True(refreshToken.IsActive);
    }

    [Fact]
    public void Create_WithDefaultExpiryDays_SetsCorrectExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var refreshToken = RefreshToken.Create(userId, tenantId);

        // Assert
        Assert.True(refreshToken.ExpiryDateUtc > now.AddDays(6.5));
        Assert.True(refreshToken.ExpiryDateUtc < now.AddDays(7.5));
    }

    [Fact]
    public void Create_GeneratesUniqueTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var token1 = RefreshToken.Create(userId, tenantId);
        var token2 = RefreshToken.Create(userId, tenantId);

        // Assert
        Assert.NotEqual(token1.Token, token2.Token);
    }

    [Fact]
    public void IsActive_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        var isActive = refreshToken.IsActive;

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_WithRevokedToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        refreshToken.Revoke();

        // Assert
        Assert.False(refreshToken.IsActive);
        Assert.True(refreshToken.IsRevoked);
        Assert.NotNull(refreshToken.RevokedDateUtc);
    }

    [Fact]
    public void IsExpired_WithFutureExpiryDate_ReturnsFalse()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), 7);

        // Act
        var isExpired = refreshToken.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void IsExpired_WithPastExpiryDate_ReturnsTrue()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Token = "test-token",
            ExpiryDateUtc = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Act
        var isExpired = refreshToken.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void IsActive_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Token = "test-token",
            ExpiryDateUtc = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Act
        var isActive = refreshToken.IsActive;

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void Revoke_ValidToken_MarksAsRevoked()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), Guid.NewGuid(), 7);
        var beforeRevoke = DateTime.UtcNow;

        // Act
        refreshToken.Revoke();

        var afterRevoke = DateTime.UtcNow;

        // Assert
        Assert.True(refreshToken.IsRevoked);
        Assert.NotNull(refreshToken.RevokedDateUtc);
        Assert.True(refreshToken.RevokedDateUtc >= beforeRevoke && refreshToken.RevokedDateUtc <= afterRevoke);
        Assert.False(refreshToken.IsActive);
    }

    [Fact]
    public void Create_GeneratesBase64EncodedToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var refreshToken = RefreshToken.Create(userId, tenantId);

        // Assert
        // Token should be decodable as base64
        var decodedBytes = Convert.FromBase64String(refreshToken.Token);
        Assert.NotEmpty(decodedBytes);
        Assert.Equal(32, decodedBytes.Length); // 32 bytes = 256 bits
    }
}
