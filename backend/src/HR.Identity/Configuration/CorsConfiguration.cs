namespace HR.Identity.Configuration;

using Microsoft.Extensions.DependencyInjection;

public static class CorsConfiguration
{
    public static IServiceCollection AddIdentityCors(this IServiceCollection services)
    {
        services.AddCors(options => { options.AddPolicy("AllowAll", policyBuilder => { policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }); });
        return services;
    }
}
