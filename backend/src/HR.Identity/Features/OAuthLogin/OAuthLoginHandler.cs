namespace HR.Identity.Features.OAuthLogin;

using MediatR;
using HR.Identity.Application.Dtos.Login;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;
using HR.Identity.Configuration;
using HR.Identity.Domain;
using HR.Common;

/// <summary>
/// Handler for OAuth login
/// </summary>
public class OAuthLoginHandler : IRequestHandler<OAuthLoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly OAuthOptions _oauthOptions;
    private readonly ILogger<OAuthLoginHandler> _logger;

    public OAuthLoginHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IOAuthProviderFactory oauthProviderFactory,
        IOptions<OAuthOptions> oauthOptions,
        ILogger<OAuthLoginHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _oauthProviderFactory = oauthProviderFactory;
        _oauthOptions = oauthOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var providerType = (OAuthProviderType)request.ProviderType;
            _logger.LogInformation("OAuth login attempt with provider: {Provider}", providerType);

            if (!_oauthOptions.Enabled)
            {
                throw new InvalidOperationException("OAuth service is not enabled");
            }

            // Get the OAuth provider
            var provider = _oauthProviderFactory.GetProvider(request.ProviderType);
            if (provider == null)
            {
                throw new InvalidOperationException($"OAuth provider {providerType} is not supported");
            }

            // Validate OAuth token and get user info
            OAuthUserInfo? userInfo = null;

            if (!string.IsNullOrEmpty(request.AccessToken))
            {
                userInfo = await provider.ValidateAndGetUserAsync(request.AccessToken, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.IdToken))
            {
                userInfo = await provider.ValidateIdTokenAsync(request.IdToken, cancellationToken);
            }

            if (userInfo == null)
            {
                _logger.LogError("Failed to validate OAuth token for provider: {Provider}", providerType);
                throw new InvalidOperationException("OAuth token validation failed");
            }

            _logger.LogInformation("OAuth user validated. Email: {Email}, ProviderId: {ProviderId}", userInfo.Email, userInfo.ProviderId);

            // Find or create user
            var userRepository = _unitOfWork.GetRepository<User>();
            var user = await userRepository.FirstOrDefaultAsync(u => u.Email == userInfo.Email, cancellationToken);

            if (user == null)
            {
                if (!_oauthOptions.AllowAutoRegistration)
                {
                    throw new InvalidOperationException("User does not exist and auto-registration is disabled");
                }

                // Create new user
                user = User.Create(
                    userInfo.Email,
                    userInfo.Name,
                    "oauth_user");

                _logger.LogInformation("Creating new user from OAuth: {Email}", userInfo.Email);
                userRepository.Add(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Link OAuth provider to user
            var oauthProvider = OAuthProvider.Create(
                user.Id,
                providerType,
                userInfo.ProviderId,
                userInfo.Email,
                userInfo.Name,
                userInfo.ProfilePictureUrl,
                Guid.NewGuid()); // TODO: Get actual tenant ID

            var oauthProviderRepository = _unitOfWork.GetRepository<OAuthProvider>();
            var existingProvider = await oauthProviderRepository.FirstOrDefaultAsync(
                p => p.UserId == user.Id && p.ProviderType == providerType,
                cancellationToken);

            if (existingProvider == null)
            {
                oauthProviderRepository.Add(oauthProvider);
            }
            else
            {
                existingProvider.UpdateTokens(request.AccessToken ?? request.IdToken ?? "");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _tokenService.GenerateAccessToken(user, new List<string> { "User" });
            var refreshToken = Guid.NewGuid().ToString();

            _logger.LogInformation("OAuth login successful for user: {Email}", user.Email);

            return new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OAuth login failed for provider: {Provider}", request.ProviderType);
            throw;
        }
    }
}
