namespace HR.Common.Behaviors;

using System.Reflection;
using StackExchange.Redis;

/// <summary>
/// Attribute to mark queries as cacheable.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CacheableQueryAttribute : Attribute
{
    public string? CacheKey { get; set; }
    public int DurationSeconds { get; set; } = 300; // Default 5 minutes
}

/// <summary>
/// MediatR pipeline behavior for query caching.
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CachingBehavior(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheAttribute = typeof(TRequest).GetCustomAttribute<CacheableQueryAttribute>();

        if (cacheAttribute is null)
        {
            return await next();
        }

        var db = _connectionMultiplexer.GetDatabase();
        var cacheKey = GenerateCacheKey(cacheAttribute.CacheKey, request);

        var cachedValue = await db.StringGetAsync(cacheKey);
        if (cachedValue.HasValue && typeof(TResponse) == typeof(string))
        {
            return (TResponse)(object)cachedValue.ToString()!;
        }

        var response = await next();

        if (response is not null)
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await db.StringSetAsync(
                cacheKey,
                serialized,
                TimeSpan.FromSeconds(cacheAttribute.DurationSeconds));
        }

        return response;
    }

    private static string GenerateCacheKey(string? baseKey, TRequest request)
    {
        var key = baseKey ?? typeof(TRequest).Name;
        var properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase);
        var keyParts = new List<string> { key };

        foreach (var prop in properties)
        {
            var value = prop.GetValue(request);
            if (value is not null)
            {
                keyParts.Add($"{prop.Name}={value}");
            }
        }

        return string.Join(":", keyParts);
    }
}
