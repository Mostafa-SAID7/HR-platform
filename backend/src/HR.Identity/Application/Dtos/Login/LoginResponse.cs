namespace HR.Identity.Application.Dtos.Login;

/// <summary>
/// Login response DTO with JWT token.
/// </summary>
public record LoginResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } // seconds
    public List<string> Roles { get; set; } = [];
}
