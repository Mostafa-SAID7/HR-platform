namespace HR.Common.Mapping;

using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for Mapster registration and configuration.
/// </summary>
public static class MapsterExtensions
{
    /// <summary>
    /// Register Mapster with auto-configuration of IMapFrom implementations.
    /// </summary>
    public static IServiceCollection AddMapster(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assemblies);

        // Register all IMapFrom implementations
        var mapFromType = typeof(IMapFrom<>);
        var allTypes = assemblies.SelectMany(a => a.GetLoadableTypes());

        foreach (var type in allTypes)
        {
            var interfaces = type.GetInterfaces();
            var mapFromInterfaces = interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType)
                .ToList();

            foreach (var mapFromInterface in mapFromInterfaces)
            {
                var sourceType = mapFromInterface.GetGenericArguments()[0];
                var instance = Activator.CreateInstance(type) as IMapFrom<object>;
                var config2 = TypeAdapterConfig.GlobalSettings;
                instance?.Mapping(config2);
            }
        }

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    /// <summary>
    /// Get all loadable types from assembly.
    /// </summary>
    private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.OfType<Type>();
        }
    }
}
