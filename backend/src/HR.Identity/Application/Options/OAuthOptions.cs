namespace HR.Identity.Application.Options;

/// <summary>
/// OAuth global configuration options
/// Supported providers: Google, Facebook
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "OAuth";

    public bool Enabled { get; set; } = true;
    public string[] AllowedProviders { get; set; } = [ "Google", "Facebook" ];
    public int CookieExpiration { get; set; } = 14; // Days
    public bool AllowAutoRegistration { get; set; } = true;
}
