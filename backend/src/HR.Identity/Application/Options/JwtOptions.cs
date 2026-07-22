namespace HR.Identity.Application.Options;

/// <summary>
/// JWT configuration options
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationSeconds { get; set; } = 3600;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
