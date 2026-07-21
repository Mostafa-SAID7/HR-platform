namespace HR.Identity.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using HR.Common;
using HR.Identity.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, IdentityDbContext>();
        services.AddOutboxPattern<IdentityDbContext>();
        services.AddCommonServices(Assembly.GetExecutingAssembly(), typeof(Program).Assembly);
        services.AddRedisCache("localhost:6379");
        return services;
    }
}
