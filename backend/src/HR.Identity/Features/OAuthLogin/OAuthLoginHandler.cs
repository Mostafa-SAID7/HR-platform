namespace HR.Identity.Features.OAuthLogin;

using MediatR;
using HR.Identity.Application.Dtos;
using HR.Identity.Application.Services;
using HR.Identity.Domain;
using HR.Common;

/// <summary>
/// Handler for OAuth login
/// </summary>
public class OAuthLoginHandler : IRequestHandler<OAuthLoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<OAuthLoginHandler> _logger;

    public OAuthLoginHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<OAuthLoginHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var providerType = (OAuthProviderType)request.ProviderType;
            _logger.LogInformation("OAuth login attempt with provider: {Provider}", providerType);

            // TODO: Validate OAuth token with provider (Google, Facebook, etc.)
            // TODO: Extract user info from OAuth provider
            // TODO: Find or create user
            // TODO: Link OAuth provider to user
            // TODO: Generate JWT token

            // Placeholder response
            var response = new LoginResponse(
                Guid.NewGuid(),
                "user@example.com",
                "JWT_TOKEN_HERE",
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddDays(7));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed for provider: {Provider}", request.ProviderType);
            throw;
        }
    }
}
