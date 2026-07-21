namespace HR.Identity.Domain;

/// <summary>
/// Role claim entity.
/// </summary>
public class RoleClaim : BaseEntity
{
    public Guid RoleId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public Role? Role { get; set; }
}
