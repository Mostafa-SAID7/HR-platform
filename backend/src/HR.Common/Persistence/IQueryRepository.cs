namespace HR.Common.Persistence;

/// <summary>
/// Read-only query repository interface for Dapper-based complex queries.
/// </summary>
public interface IQueryRepository
{
    /// <summary>
    /// Execute a raw SQL query and map results to TResult.
    /// </summary>
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a raw SQL query and return a single result.
    /// </summary>
    Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a raw SQL query and return the first result.
    /// </summary>
    Task<TResult?> QueryFirstOrDefaultAsync<TResult>(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a raw SQL command (INSERT, UPDATE, DELETE).
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure and map results to TResult.
    /// </summary>
    Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(string procedureName, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a stored procedure without returning results.
    /// </summary>
    Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk insert records (optimized for large datasets).
    /// </summary>
    Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, string tableName, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Bulk update records (optimized for large datasets).
    /// </summary>
    Task BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entities, string tableName, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Bulk delete records (optimized for large datasets).
    /// </summary>
    Task BulkDeleteAsync(string tableName, string whereClause, object? parameters = null, CancellationToken cancellationToken = default);
}
