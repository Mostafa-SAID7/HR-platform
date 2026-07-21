namespace HR.Performance.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Performance.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering Performance service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Performance service dependencies
    /// </summary>
    public static IServiceCollection AddPerformanceService(
        this IServiceCollection services,
        string connectionString)
    {
        // Register database context
        services.AddPerformanceDatabase(connectionString);

        // Register application services
        services.AddPerformanceApplicationServices();

        // Register infrastructure services
        services.AddPerformanceInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddPerformanceDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PerformanceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, PerformanceDbContext>();
        services.AddOutboxPattern<PerformanceDbContext>();

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddPerformanceApplicationServices(
        this IServiceCollection services)
    {
        // Register MediatR for CQRS
        services.AddCommonServices(
            Assembly.GetExecutingAssembly(),
            typeof(Program).Assembly);

        return services;
    }

    /// <summary>
    /// Register infrastructure services (caching, queries, messaging)
    /// </summary>
    private static IServiceCollection AddPerformanceInfrastructureServices(
        this IServiceCollection services)
    {
        // Register Redis cache
        services.AddRedisCache("localhost:6379");

        return services;
    }
}
