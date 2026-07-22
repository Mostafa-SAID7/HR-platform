namespace HR.Identity.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using HR.Identity.Domain;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;

/// <summary>
/// Implementation of token service using JWT.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IOptions<JwtOptions> _options;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IOptions<JwtOptions> options, ILogger<JwtTokenService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, List<string> roles)
    {
        var payload = new JwtTokenPayload
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            Claims = user.UserClaims.ToDictionary(uc => uc.ClaimType, uc => uc.ClaimValue)
        };

        return GenerateAccessToken(payload);
    }

    public string GenerateAccessToken(JwtTokenPayload payload)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
            new Claim(ClaimTypes.Email, payload.Email),
            new Claim(ClaimTypes.Name, payload.FullName),
            new Claim("tenant_id", payload.TenantId.ToString())
        };

        // Add roles as claims
        foreach (var role in payload.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        foreach (var claim in payload.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Value.Issuer,
            audience: _options.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_options.Value.AccessTokenExpirationSeconds),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtSecurityToken? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                _logger.LogWarning("Cannot read token");
                return null;
            }

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _options.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Value.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public Dictionary<string, string> ExtractClaims(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims
                .ToDictionary(c => c.Type, c => c.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract claims from token");
            return new Dictionary<string, string>();
        }
    }
}
