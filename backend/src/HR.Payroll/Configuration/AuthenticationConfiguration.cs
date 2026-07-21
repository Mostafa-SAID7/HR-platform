namespace HR.Payroll.Configuration;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Authentication and authorization configuration for Payroll service
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    public static IServiceCollection AddPayrollAuthentication(
        this IServiceCollection services)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true
                };
            });

        return services;
    }

    /// <summary>
    /// Configure authorization policies
    /// </summary>
    public static IServiceCollection AddPayrollAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
