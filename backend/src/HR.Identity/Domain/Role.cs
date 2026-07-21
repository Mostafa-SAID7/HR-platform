namespace HR.Identity.Domain;

/// <summary>
/// Role aggregate root for role-based access control.
/// </summary>
public class Role : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }

    // Relations
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();

    /// <summary>
    /// Create a new role.
    /// </summary>
    public static Role Create(string name, string description, Guid tenantId, bool isSystemRole = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Add a permission to the role.
    /// </summary>
    public void AddPermission(Permission permission)
    {
        if (RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        RolePermissions.Add(new RolePermission
        {
            RoleId = Id,
            PermissionId = permission.Id,
            Role = this,
            Permission = permission
        });
    }

    /// <summary>
    /// Remove a permission from the role.
    /// </summary>
    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission is not null)
        {
            RolePermissions.Remove(rolePermission);
        }
    }

    /// <summary>
    /// Add a claim to the role.
    /// </summary>
    public void AddClaim(string claimType, string claimValue)
    {
        if (RoleClaims.Any(rc => rc.ClaimType == claimType && rc.ClaimValue == claimValue))
            return;

        RoleClaims.Add(new RoleClaim
        {
            Id = Guid.NewGuid(),
            RoleId = Id,
            ClaimType = claimType,
            ClaimValue = claimValue,
            Role = this
        });
    }
}

/// <summary>
/// Permission entity for fine-grained access control.
/// </summary>
public class Permission : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., "Employee", "Performance"
    public string Action { get; set; } = string.Empty;   // e.g., "Create", "Read", "Update", "Delete"

    // Relations
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Create a new permission.
    /// </summary>
    public static Permission Create(string name, string description, string resource, string action, Guid tenantId)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Resource = resource,
            Action = action,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Role-Permission junction entity.
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

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
