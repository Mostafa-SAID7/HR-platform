namespace HR.Identity.Features.Login;

using MediatR;
using HR.Identity.Application.Dtos.Login;
using HR.Identity.Application.Mappings;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userRepository = _unitOfWork.GetRepository<User>();
        var user = await userRepository.GetAsQueryable()
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login failed for email {Email}: User not found", request.Email);
            throw new NotFoundException("User", request.Email);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed for email {Email}: User is inactive", request.Email);
            throw new ForbiddenException("User account is inactive");
        }

        if (user.IsLockedOut)
        {
            _logger.LogWarning("Login failed for email {Email}: User is locked out", request.Email);
            throw new ForbiddenException("User account is locked. Please try again later.");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLoginAttempt();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Login failed for email {Email}: Invalid password", request.Email);
            throw new ForbiddenException("Invalid email or password");
        }

        user.ResetLoginAttempts();

        var roles = user.UserRoles
            .Select(ur => ur.Role?.Name ?? string.Empty)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList();

        var refreshToken = RefreshToken.Create(user.Id, user.TenantId, expiryDays: 7);
        user.RefreshTokens.Add(refreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        // Use centralized mapping instead of inline
        return user.ToLoginResponse(accessToken, refreshToken.Token, roles);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
