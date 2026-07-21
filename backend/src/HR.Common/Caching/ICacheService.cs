namespace HR.Common.Caching;

/// <summary>
/// Cache service interface for distributed caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a value from cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a value in cache with expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a value from cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove multiple values from cache.
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or set a value in cache (get if exists, otherwise set).
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all cache.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment a numeric value in cache.
    /// </summary>
    Task<long> IncrementAsync(string key, long increment = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrement a numeric value in cache.
    /// </summary>
    Task<long> DecrementAsync(string key, long decrement = 1, CancellationToken cancellationToken = default);
}
