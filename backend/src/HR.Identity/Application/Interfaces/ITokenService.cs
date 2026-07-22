namespace HR.Identity.Application.Interfaces;

using HR.Identity.Domain;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate JWT access token
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Get claims from token
    /// </summary>
    Dictionary<string, string> GetClaimsFromToken(string token);
}
