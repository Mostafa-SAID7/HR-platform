namespace HR.Identity.Features.Register;

using HR.Identity.Application.Dtos.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterCommand(
    RegisterRequest Request,
    Guid TenantId) : ICommand<RegisterResponse>;
