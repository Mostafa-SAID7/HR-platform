namespace HR.Identity.Features.RefreshToken;

using HR.Identity.Domain;
using HR.Identity.Application.Services;
using HR.Identity.Application.Dtos.RefreshToken;
using HR.Common;

/// <summary>
/// Handler for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRepository<Domain.RefreshToken> _refreshTokenRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRepository<Domain.RefreshToken> refreshTokenRepository,
        IRepository<User> userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing token");

        // Validate access token format (basic validation)
        var validatedToken = _tokenService.ValidateToken(request.Request.AccessToken);
        if (validatedToken == null)
        {
            throw new ValidationException("Invalid access token");
        }

        // Extract user ID from token
        var userIdClaim = validatedToken.Claims.FirstOrDefault(c => c.Type == "nameid");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new ValidationException("Invalid token claims");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new ValidationException("User not found or inactive");
        }

        // Verify refresh token
        var refreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(
            rt => rt.Token == request.Request.RefreshToken && rt.UserId == userId,
            cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new ValidationException("Invalid or expired refresh token");
        }

        // Get user roles for new token
        var userRoles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList();

        // Generate new access token
        var newAccessToken = _tokenService.GenerateAccessToken(user, userRoles);

        // Create new refresh token
        var newRefreshToken = Domain.RefreshToken.Create(userId, request.TenantId, 7);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // Revoke old refresh token
        refreshToken.Revoke();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken.Token);
    }
}
