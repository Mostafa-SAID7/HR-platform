namespace HR.Identity.Application.Services;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;

/// <summary>
/// Google OAuth provider implementation
/// </summary>
public class GoogleOAuthProvider : IOAuthProvider
{
    private readonly GoogleOAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthProvider> _logger;

    public string ProviderName => "Google";

    public GoogleOAuthProvider(
        IOptions<GoogleOAuthOptions> options,
        HttpClient httpClient,
        ILogger<GoogleOAuthProvider> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OAuthUserInfo?> ValidateAndGetUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating Google access token");

            var request = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google validation failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var userInfo = await response.Content.ReadAsAsync<GoogleUserInfo>(cancellationToken);

            if (userInfo == null)
            {
                _logger.LogError("Could not parse Google user info");
                return null;
            }

            return new OAuthUserInfo(
                userInfo.Id,
                userInfo.Email,
                userInfo.Name ?? "Google User",
                userInfo.Picture,
                new Dictionary<string, string>
                {
                    { "email_verified", userInfo.Verified_Email.ToString() }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google access token");
            return null;
        }
    }

    public async Task<OAuthUserInfo?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating Google ID token");

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var picture = token.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            var sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub))
            {
                _logger.LogError("Required claims missing from Google ID token");
                return null;
            }

            return new OAuthUserInfo(
                sub,
                email,
                name ?? "Google User",
                picture,
                new Dictionary<string, string>
                {
                    { "email_verified", token.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value ?? "false" }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google ID token");
            return null;
        }
    }

    private class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public bool Verified_Email { get; set; }
    }
}
