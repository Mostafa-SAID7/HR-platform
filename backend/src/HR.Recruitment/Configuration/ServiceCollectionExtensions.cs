namespace HR.Recruitment.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Recruitment.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering Recruitment service dependencies
/// Organized by concern following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all Recruitment service dependencies
    /// </summary>
    public static IServiceCollection AddRecruitmentService(
        this IServiceCollection services,
        string connectionString)
    {
        // Register database context
        services.AddRecruitmentDatabase(connectionString);

        // Register application services
        services.AddRecruitmentApplicationServices();

        // Register infrastructure services
        services.AddRecruitmentInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Register database and persistence services
    /// </summary>
    private static IServiceCollection AddRecruitmentDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<RecruitmentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, RecruitmentDbContext>();
        services.AddOutboxPattern<RecruitmentDbContext>();

        return services;
    }

    /// <summary>
    /// Register application layer services (MediatR, validators, mappers)
    /// </summary>
    private static IServiceCollection AddRecruitmentApplicationServices(
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
    private static IServiceCollection AddRecruitmentInfrastructureServices(
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
