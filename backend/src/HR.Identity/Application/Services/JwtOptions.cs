namespace HR.Identity.Application.Services;

/// <summary>
/// JWT token options and configuration.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "https://identityservice";
    public string Audience { get; set; } = "hranalytics";
    public int AccessTokenExpirationSeconds { get; set; } = 3600; // 1 hour
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
