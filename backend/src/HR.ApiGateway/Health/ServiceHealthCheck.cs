using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HR.ApiGateway.Health;

/// <summary>
/// Health check for the API Gateway service.
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly string _serviceName;

    public ServiceHealthCheck(string serviceName)
    {
        _serviceName = serviceName;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = HealthCheckResult.Healthy($"{_serviceName} is running");
        return Task.FromResult(result);
    }
}
