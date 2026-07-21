namespace HR.Attendance.Configuration;

using Microsoft.Extensions.DependencyInjection;
using HR.Common;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddAttendanceHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMicroserviceHealthChecks(
                "Attendance Service",
                postgresConnectionString: configuration.GetConnectionString("DefaultConnection"));
        return services;
    }
}
