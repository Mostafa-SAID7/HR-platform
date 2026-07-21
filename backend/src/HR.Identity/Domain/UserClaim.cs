namespace HR.Identity.Domain;

/// <summary>
/// User claim entity.
/// </summary>
public class UserClaim : BaseEntity
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public User? User { get; set; }
}
