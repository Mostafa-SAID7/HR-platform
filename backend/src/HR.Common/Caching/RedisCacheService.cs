namespace HR.Common.Caching;

using StackExchange.Redis;
using System.Text.Json;

/// <summary>
/// Redis implementation of the cache service.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly IServer _server;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
        _server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().FirstOrDefault()!);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serialized, expiration ?? TimeSpan.FromHours(1));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = _server.Keys(pattern: pattern).ToArray();

        if (keys.Length > 0)
        {
            await _database.KeyDeleteAsync(keys);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue is not null)
            return cachedValue;

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            await server.FlushDatabaseAsync();
        }
    }

    public async Task<long> IncrementAsync(string key, long increment = 1, CancellationToken cancellationToken = default)
    {
        return await _database.StringIncrementAsync(key, increment);
    }

    public async Task<long> DecrementAsync(string key, long decrement = 1, CancellationToken cancellationToken = default)
    {
        return await _database.StringDecrementAsync(key, decrement);
    }
}
