namespace HR.Identity.Application.Mappings;

using HR.Identity.Domain;
using HR.Identity.Application.Dtos.UserProfile;

/// <summary>
/// Centralized mapping configuration for UserProfile DTOs.
/// </summary>
public static class UserProfileDtoMappingConfiguration
{
    public static UserProfileDto ToProfileDto(this User user, List<string> roles)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Department = user.Department,
            JobTitle = user.JobTitle,
            Roles = roles,
            LastLoginUtc = user.LastLoginUtc
        };
    }
}
