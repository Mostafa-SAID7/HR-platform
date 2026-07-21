namespace HR.Identity.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddIdentityHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddMicroserviceHealthChecks("Identity Service", postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
