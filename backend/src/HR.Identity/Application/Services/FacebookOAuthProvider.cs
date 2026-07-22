namespace HR.Identity.Application.Services;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;

/// <summary>
/// Facebook OAuth provider implementation
/// </summary>
public class FacebookOAuthProvider : IOAuthProvider
{
    private readonly FacebookOAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<FacebookOAuthProvider> _logger;

    public string ProviderName => "Facebook";

    public FacebookOAuthProvider(
        IOptions<FacebookOAuthOptions> options,
        HttpClient httpClient,
        ILogger<FacebookOAuthProvider> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OAuthUserInfo?> ValidateAndGetUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating Facebook access token");

            var url = $"{_options.UserInfoEndpoint}&access_token={accessToken}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook validation failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var userInfo = await response.Content.ReadAsAsync<FacebookUserInfo>(cancellationToken);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.Id))
            {
                _logger.LogError("Could not parse Facebook user info");
                return null;
            }

            var profilePictureUrl = userInfo.Picture?.Data?.Url;

            return new OAuthUserInfo(
                userInfo.Id,
                userInfo.Email ?? "",
                userInfo.Name ?? "Facebook User",
                profilePictureUrl,
                new Dictionary<string, string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Facebook access token");
            return null;
        }
    }

    public async Task<OAuthUserInfo?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Facebook OAuth does not support ID token validation. Use access token instead.");
        return await Task.FromResult<OAuthUserInfo?>(null);
    }

    private class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Name { get; set; }
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        public string? Url { get; set; }
    }
}
