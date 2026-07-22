namespace HR.Identity.Application.Options;

/// <summary>
/// Google OAuth configuration
/// </summary>
public class GoogleOAuthOptions
{
    public const string SectionName = "OAuth:Google";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
    public string UserInfoEndpoint { get; set; } = "https://www.googleapis.com/oauth2/v2/userinfo";
    public string[] Scopes { get; set; } = [ "openid", "profile", "email" ];
    public string CallbackPath { get; set; } = "/signin-google";
}
