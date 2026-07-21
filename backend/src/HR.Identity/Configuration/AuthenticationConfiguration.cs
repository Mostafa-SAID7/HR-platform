namespace HR.Identity.Configuration;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("Bearer").AddJwtBearer(options =>
        {
            options.Authority = "https://localhost:5001";
            options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false, ValidateIssuer = false, ValidateLifetime = true };
        });
        return services;
    }

    public static IServiceCollection AddIdentityAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
