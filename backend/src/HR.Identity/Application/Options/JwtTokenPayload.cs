namespace HR.Identity.Application.Options;

/// <summary>
/// JWT token payload model for claim generation
/// </summary>
public class JwtTokenPayload
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public Dictionary<string, string> Claims { get; set; } = [];
}
