namespace HR.Identity.Features.Login;

using HR.Identity.Application.Dtos.Login;

public record LoginCommand(string Email, string Password, bool RememberMe) : ICommand<LoginResponse>;
