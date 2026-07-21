namespace HR.Audit.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddAuditHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddMicroserviceHealthChecks("Audit Service", postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
