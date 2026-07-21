namespace HR.Common.Caching;

/// <summary>
/// Builder for generating cache keys with consistent naming conventions.
/// </summary>
public static class CacheKeyBuilder
{
    private const string Separator = ":";

    /// <summary>
    /// Build a cache key from parts.
    /// </summary>
    public static string Build(params string[] parts)
    {
        return string.Join(Separator, parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    /// <summary>
    /// Build a cache key for an entity by ID.
    /// </summary>
    public static string EntityById(string entityName, Guid id)
    {
        return Build("entity", entityName, id.ToString());
    }

    /// <summary>
    /// Build a cache key for a list of entities.
    /// </summary>
    public static string EntityList(string entityName, int pageNumber = 1, int pageSize = 10)
    {
        return Build("list", entityName, $"page{pageNumber}size{pageSize}");
    }

    /// <summary>
    /// Build a cache key for filtered results.
    /// </summary>
    public static string FilteredList(string entityName, string filter, int pageNumber = 1, int pageSize = 10)
    {
        return Build("filtered", entityName, filter, $"page{pageNumber}size{pageSize}");
    }

    /// <summary>
    /// Build a cache key for search results.
    /// </summary>
    public static string SearchResults(string entityName, string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        return Build("search", entityName, searchTerm, $"page{pageNumber}size{pageSize}");
    }

    /// <summary>
    /// Build a cache key for count statistics.
    /// </summary>
    public static string Count(string entityName)
    {
        return Build("count", entityName);
    }

    /// <summary>
    /// Build a cache key for aggregated data.
    /// </summary>
    public static string Aggregate(string entityName, string aggregationType)
    {
        return Build("aggregate", entityName, aggregationType);
    }

    /// <summary>
    /// Get a pattern for cache invalidation.
    /// </summary>
    public static string Pattern(string entityName)
    {
        return Build("*", entityName, "*");
    }
}
