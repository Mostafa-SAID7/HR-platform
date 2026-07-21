namespace HR.Employee.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Employee.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering Employee service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Employee service dependencies
    /// </summary>
    public static IServiceCollection AddEmployeeService(
        this IServiceCollection services,
        string connectionString)
    {
        // Register database context
        services.AddEmployeeDatabase(connectionString);

        // Register application services
        services.AddEmployeeApplicationServices();

        // Register infrastructure services
        services.AddEmployeeInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddEmployeeDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EmployeeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, EmployeeDbContext>();
        services.AddOutboxPattern<EmployeeDbContext>();

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddEmployeeApplicationServices(
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
    private static IServiceCollection AddEmployeeInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis cache
        services.AddRedisCache(
            configuration.GetConnectionString("Redis") ?? "localhost:6379");

        // Register Dapper query repository
        services.AddScoped<IQueryRepository>(provider =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var connection = new Npgsql.NpgsqlConnection(connectionString);
            return new DapperQueryRepository(connection);
        });

        // Register Kafka messaging
        services.AddKafkaMessaging(configuration, Assembly.GetExecutingAssembly());

        return services;
    }
}
