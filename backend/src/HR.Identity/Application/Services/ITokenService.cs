namespace HR.Identity.Application.Services;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate an access token for a user.
    /// </summary>
    string GenerateAccessToken(User user, List<string> roles);

    /// <summary>
    /// Generate a token from claims.
    /// </summary>
    string GenerateAccessToken(JwtTokenPayload payload);

    /// <summary>
    /// Validate and decode a JWT token.
    /// </summary>
    JwtSecurityToken? ValidateToken(string token);

    /// <summary>
    /// Extract claims from a token.
    /// </summary>
    Dictionary<string, string> ExtractClaims(string token);
}
