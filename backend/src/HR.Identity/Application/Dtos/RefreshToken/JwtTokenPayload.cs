namespace HR.Identity.Application.Dtos.RefreshToken;

/// <summary>
/// JWT token payload DTO.
/// </summary>
public record JwtTokenPayload
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public Dictionary<string, string> Claims { get; set; } = [];
}
