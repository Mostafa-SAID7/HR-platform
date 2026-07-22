namespace HR.Identity.Application.Mappings;

using Mapster;
using HR.Identity.Domain;
using HR.Identity.Application.Dtos.Register;

/// <summary>
/// Centralized DTO mapping configuration for Register aggregate.
/// </summary>
public static class RegisterDtoMappingConfiguration
{
    /// <summary>
    /// Map User domain entity to RegisterResponse DTO.
    /// </summary>
    public static RegisterResponse ToRegisterResponse(this User user)
    {
        return new RegisterResponse(
            user.Id,
            user.Email,
            user.Username,
            user.FullName,
            "User registered successfully");
    }

    /// <summary>
    /// Map RegisterRequest to User domain entity.
    /// </summary>
    public static User ToUserDomain(this RegisterRequest request, string passwordHash, Guid tenantId)
    {
        return User.Create(
            request.Email,
            request.Username,
            request.FullName,
            passwordHash,
            tenantId);
    }
}
