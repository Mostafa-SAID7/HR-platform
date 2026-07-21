namespace HR.Attendance.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Attendance.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering Attendance service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Attendance service dependencies
    /// </summary>
    public static IServiceCollection AddAttendanceService(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddAttendanceDatabase(connectionString);
        services.AddAttendanceApplicationServices();
        services.AddAttendanceInfrastructureServices(configuration);
        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddAttendanceDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, AttendanceDbContext>();
        services.AddOutboxPattern<AttendanceDbContext>();
        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddAttendanceApplicationServices(
        this IServiceCollection services)
    {
        services.AddCommonServices(
            Assembly.GetExecutingAssembly(),
            typeof(Program).Assembly);
        return services;
    }

    /// <summary>
    /// Register infrastructure services (caching, SignalR, messaging)
    /// </summary>
    private static IServiceCollection AddAttendanceInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis cache
        services.AddRedisCache(
            configuration.GetConnectionString("Redis") ?? "localhost:6379");

        // Register SignalR
        services.AddSignalR();

        // Register Kafka messaging
        services.AddKafkaMessaging(configuration, Assembly.GetExecutingAssembly());

        return services;
    }
}
