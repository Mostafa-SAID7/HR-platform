namespace HR.Identity.Application.Options;

/// <summary>
/// Facebook OAuth configuration
/// </summary>
public class FacebookOAuthOptions
{
    public const string SectionName = "OAuth:Facebook";

    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://www.facebook.com/v18.0/dialog/oauth";
    public string TokenEndpoint { get; set; } = "https://graph.facebook.com/v18.0/oauth/access_token";
    public string UserInfoEndpoint { get; set; } = "https://graph.facebook.com/me?fields=id,email,name,picture.type(large)";
    public string[] Scopes { get; set; } = [ "email", "public_profile" ];
    public string CallbackPath { get; set; } = "/signin-facebook";
}
