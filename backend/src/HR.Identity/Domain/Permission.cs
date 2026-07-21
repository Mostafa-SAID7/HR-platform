namespace HR.Identity.Domain;

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
