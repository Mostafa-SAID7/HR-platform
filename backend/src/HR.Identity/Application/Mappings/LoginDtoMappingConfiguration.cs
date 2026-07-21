namespace HR.Identity.Application.Mappings;

using HR.Identity.Domain;
using HR.Identity.Application.Dtos.Login;

/// <summary>
/// Centralized mapping configuration for Login DTOs.
/// </summary>
public static class LoginDtoMappingConfiguration
{
    public static LoginResponse ToLoginResponse(this User user, string accessToken, string refreshToken, List<string> roles)
    {
        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour
            Roles = roles
        };
    }
}
