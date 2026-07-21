namespace HR.Identity.Application.Dtos.Role;

/// <summary>
/// Role DTO.
/// </summary>
public record RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public List<string> Permissions { get; set; } = [];
}
