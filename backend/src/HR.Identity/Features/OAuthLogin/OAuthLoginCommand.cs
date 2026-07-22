namespace HR.Identity.Features.OAuthLogin;

using MediatR;
using HR.Identity.Application.Dtos;

/// <summary>
/// Command for OAuth login (Google, Facebook, etc.)
/// </summary>
public class OAuthLoginCommand : IRequest<LoginResponse>
{
    public int ProviderType { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }

    public OAuthLoginCommand(int providerType, string accessToken, string? refreshToken = null)
    {
        ProviderType = providerType;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }
}
