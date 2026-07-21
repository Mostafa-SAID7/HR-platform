namespace HR.Identity.Application.Dtos.Login;

/// <summary>
/// Login request DTO.
/// </summary>
public record LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
