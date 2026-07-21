namespace HR.Identity.Application.Mappings;

using HR.Identity.Domain;
using HR.Identity.Application.Dtos.Permission;

/// <summary>
/// Centralized mapping configuration for Permission DTOs.
/// </summary>
public static class PermissionDtoMappingConfiguration
{
    public static PermissionDto ToDto(this Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description ?? string.Empty,
            Resource = permission.Resource,
            Action = permission.Action
        };
    }
}
