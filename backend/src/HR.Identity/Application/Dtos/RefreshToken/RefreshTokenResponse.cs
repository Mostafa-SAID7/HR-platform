namespace HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// Refresh token response DTO.
/// </summary>
public record RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
