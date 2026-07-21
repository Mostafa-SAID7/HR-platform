namespace HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// Refresh token request DTO.
/// </summary>
public record RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
