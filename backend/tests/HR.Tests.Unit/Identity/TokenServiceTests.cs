namespace HR.Tests.Unit.Identity;

/// <summary>
/// Unit tests for JWT token service.
/// Tests cover: token generation, validation, claim extraction, and expiration handling.
/// </summary>
public class TokenServiceTests
{
    private readonly JwtTokenService _tokenService;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly Mock<ILogger<JwtTokenService>> _mockLogger;

    public TokenServiceTests()
    {
        _mockLogger = new Mock<ILogger<JwtTokenService>>();

        var jwtOptions = new JwtOptions
        {
            SecretKey = "this-is-a-very-long-secret-key-that-is-at-least-256-bits-long-for-HS256-algorithm",
            Issuer = "https://identityservice",
            Audience = "hranalytics",
            AccessTokenExpirationSeconds = 3600,
            RefreshTokenExpirationDays = 7
        };

        _jwtOptions = Options.Create(jwtOptions);
        _tokenService = new JwtTokenService(_jwtOptions, _mockLogger.Object);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", tenantId)
        {
            Id = userId
        };

        var role = Role.Create("Admin", "Administrator", tenantId, isSystemRole: true);
        user.AddRole(role);
        user.AddClaim("department", "Engineering");

        var roles = new List<string> { "Admin" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(token.Length > 100); // JWT tokens are fairly long
    }

    [Fact]
    public void GenerateAccessToken_WithPayload_ReturnsValidToken()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string> { "Admin", "Manager" },
            Claims = new Dictionary<string, string>
            {
                { "department", "Engineering" },
                { "team", "Backend" }
            }
        };

        // Act
        var token = _tokenService.GenerateAccessToken(payload);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsDecodedToken()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string> { "Admin" },
            Claims = new Dictionary<string, string>()
        };

        var token = _tokenService.GenerateAccessToken(payload);

        // Act
        var validatedToken = _tokenService.ValidateToken(token);

        // Assert
        Assert.NotNull(validatedToken);
        Assert.Equal(_jwtOptions.Value.Issuer, validatedToken.Issuer);
        Assert.Equal(_jwtOptions.Value.Audience, validatedToken.Audiences.First());
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var validatedToken = _tokenService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(validatedToken);
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var malformedToken = "not.a.valid.jwt.token";

        // Act
        var validatedToken = _tokenService.ValidateToken(malformedToken);

        // Assert
        Assert.Null(validatedToken);
    }

    [Fact]
    public void ValidateToken_WithTamperedSignature_ReturnsNull()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string>(),
            Claims = new Dictionary<string, string>()
        };

        var token = _tokenService.GenerateAccessToken(payload);
        var parts = token.Split('.');
        var tamperedToken = $"{parts[0]}.{parts[1]}.tamperedsignature";

        // Act
        var validatedToken = _tokenService.ValidateToken(tamperedToken);

        // Assert
        Assert.Null(validatedToken);
    }

    [Fact]
    public void ExtractClaims_WithValidToken_ReturnsClaims()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string> { "Admin" },
            Claims = new Dictionary<string, string>
            {
                { "department", "Engineering" },
                { "team", "Backend" }
            }
        };

        var token = _tokenService.GenerateAccessToken(payload);

        // Act
        var claims = _tokenService.ExtractClaims(token);

        // Assert
        Assert.NotNull(claims);
        Assert.Contains("department", claims.Keys);
        Assert.Equal("Engineering", claims["department"]);
        Assert.Contains("team", claims.Keys);
        Assert.Equal("Backend", claims["team"]);
    }

    [Fact]
    public void ExtractClaims_WithValidToken_IncludesStandardClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var payload = new JwtTokenPayload
        {
            UserId = userId,
            TenantId = tenantId,
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string> { "Admin" },
            Claims = new Dictionary<string, string>()
        };

        var token = _tokenService.GenerateAccessToken(payload);

        // Act
        var claims = _tokenService.ExtractClaims(token);

        // Assert
        Assert.NotNull(claims);
        Assert.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", claims.Keys);
        Assert.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", claims.Keys);
        Assert.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", claims.Keys);
        Assert.Contains("tenant_id", claims.Keys);
    }

    [Fact]
    public void ExtractClaims_WithInvalidToken_ReturnsEmptyDictionary()
    {
        // Arrange
        var invalidToken = "invalid.token";

        // Act
        var claims = _tokenService.ExtractClaims(invalidToken);

        // Assert
        Assert.NotNull(claims);
        Assert.Empty(claims);
    }

    [Fact]
    public void GenerateAccessToken_IncludesRolesAsClaims()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string> { "Admin", "Manager", "User" },
            Claims = new Dictionary<string, string>()
        };

        var token = _tokenService.GenerateAccessToken(payload);

        // Act
        var validatedToken = _tokenService.ValidateToken(token);

        // Assert
        Assert.NotNull(validatedToken);
        var roleClaims = validatedToken.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        Assert.Equal(3, roleClaims.Count);
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Manager", roleClaims);
        Assert.Contains("User", roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_WithEmptyRoles_GeneratesToken()
    {
        // Arrange
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string>(),
            Claims = new Dictionary<string, string>()
        };

        // Act
        var token = _tokenService.GenerateAccessToken(payload);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateAccessToken_TokenExpiration_IsSetCorrectly()
    {
        // Arrange
        var beforeGeneration = DateTime.UtcNow;
        var payload = new JwtTokenPayload
        {
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Roles = new List<string>(),
            Claims = new Dictionary<string, string>()
        };

        // Act
        var token = _tokenService.GenerateAccessToken(payload);
        var validatedToken = _tokenService.ValidateToken(token);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        Assert.NotNull(validatedToken);
        var expectedExpiration = beforeGeneration.AddSeconds(_jwtOptions.Value.AccessTokenExpirationSeconds);
        var actualExpiration = validatedToken.ValidTo;

        // Allow 5 second tolerance for test execution time
        Assert.True(actualExpiration >= expectedExpiration.AddSeconds(-5));
        Assert.True(actualExpiration <= expectedExpiration.AddSeconds(5));
    }
}
