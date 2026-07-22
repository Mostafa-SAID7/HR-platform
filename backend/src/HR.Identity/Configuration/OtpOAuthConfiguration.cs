namespace HR.Identity.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;
using HR.Identity.Application.Services;

/// <summary>
/// Configuration for OTP (SendGrid email) and OAuth (Google, Facebook) services
/// </summary>
public static class OtpOAuthConfiguration
{
    /// <summary>
    /// Add OTP and OAuth services to the container
    /// </summary>
    public static IServiceCollection AddOtpOAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure OTP options (SendGrid email only)
        services.Configure<OtpOptions>(configuration.GetSection(OtpOptions.SectionName));
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.SectionName));

        // Configure OAuth options (Google & Facebook only)
        services.Configure<OAuthOptions>(configuration.GetSection(OAuthOptions.SectionName));
        services.Configure<GoogleOAuthOptions>(configuration.GetSection(GoogleOAuthOptions.SectionName));
        services.Configure<FacebookOAuthOptions>(configuration.GetSection(FacebookOAuthOptions.SectionName));

        // Register OTP sender - SendGrid only for email
        var otpOptions = configuration.GetSection(OtpOptions.SectionName).Get<OtpOptions>();
        if (otpOptions?.Enabled == true)
        {
            services.AddScoped<IOtpSender, SendGridOtpSender>();
        }

        // Register OAuth providers - Google & Facebook only
        var oauthOptions = configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>();
        if (oauthOptions?.Enabled == true)
        {
            services.AddHttpClient<GoogleOAuthProvider>();
            services.AddHttpClient<FacebookOAuthProvider>();

            // Register provider factory
            services.AddScoped<IOAuthProviderFactory, OAuthProviderFactory>();
        }

        return services;
    }
}

/// <summary>
/// Factory for creating OAuth providers (Google, Facebook)
/// </summary>
public interface IOAuthProviderFactory
{
    IOAuthProvider? GetProvider(int providerType);
}

/// <summary>
/// Implementation of OAuth provider factory
/// </summary>
public class OAuthProviderFactory : IOAuthProviderFactory
{
    private readonly GoogleOAuthProvider _googleProvider;
    private readonly FacebookOAuthProvider _facebookProvider;
    private readonly ILogger<OAuthProviderFactory> _logger;

    public OAuthProviderFactory(
        GoogleOAuthProvider googleProvider,
        FacebookOAuthProvider facebookProvider,
        ILogger<OAuthProviderFactory> logger)
    {
        _googleProvider = googleProvider;
        _facebookProvider = facebookProvider;
        _logger = logger;
    }

    public IOAuthProvider? GetProvider(int providerType)
    {
        return providerType switch
        {
            0 => _googleProvider, // Google
            1 => _facebookProvider, // Facebook
            _ => null
        };
    }
}
