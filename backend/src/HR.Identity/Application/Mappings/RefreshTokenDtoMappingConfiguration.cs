namespace HR.Identity.Application.Mappings;

using Mapster;
using HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// Centralized DTO mapping configuration for RefreshToken aggregate.
/// </summary>
public static class RefreshTokenDtoMappingConfiguration
{
    /// <summary>
    /// Map tokens to RefreshTokenResponse DTO.
    /// </summary>
    public static RefreshTokenResponse ToRefreshTokenResponse(string accessToken, string refreshToken)
    {
        return new RefreshTokenResponse(accessToken, refreshToken);
    }

    /// <summary>
    /// Map JWT token payload to DTO.
    /// </summary>
    public static JwtTokenPayload ToJwtTokenPayload(
        Guid userId,
        Guid tenantId,
        string email,
        string fullName,
        List<string> roles,
        Dictionary<string, string> claims)
    {
        return new JwtTokenPayload
        {
            UserId = userId,
            TenantId = tenantId,
            Email = email,
            FullName = fullName,
            Roles = roles,
            Claims = claims
        };
    }
}
