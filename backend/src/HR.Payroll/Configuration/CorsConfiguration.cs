namespace HR.Payroll.Configuration;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// CORS configuration for Payroll service
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configure CORS policies
    /// </summary>
    public static IServiceCollection AddPayrollCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowGateway", policyBuilder =>
            {
                policyBuilder
                    .WithOrigins("http://localhost:5000", "http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
