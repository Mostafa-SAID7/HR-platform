namespace HR.Identity.Application.Mappings;

using HR.Identity.Domain;
using HR.Identity.Application.Dtos.Role;

/// <summary>
/// Centralized mapping configuration for Role DTOs.
/// </summary>
public static class RoleDtoMappingConfiguration
{
    public static RoleDto ToDto(this Role role, List<string> permissions)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description ?? string.Empty,
            IsSystemRole = role.IsSystemRole,
            Permissions = permissions
        };
    }
}
