namespace HR.Common.Persistence;

using Dapper;
using System.Data;

/// <summary>
/// Query repository implementation using Dapper for complex queries and reporting.
/// </summary>
public class DapperQueryRepository : IQueryRepository
{
    private readonly IDbConnection _connection;

    public DapperQueryRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.QueryAsync<TResult>(sql, parameters);
    }

    public async Task<TResult?> QuerySingleOrDefaultAsync<TResult>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.QuerySingleOrDefaultAsync<TResult>(sql, parameters);
    }

    public async Task<TResult?> QueryFirstOrDefaultAsync<TResult>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.QueryFirstOrDefaultAsync<TResult>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.ExecuteAsync(sql, parameters);
    }

    public async Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(
        string procedureName,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.QueryAsync<TResult>(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteStoredProcedureNonQueryAsync(
        string procedureName,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        return await _connection.ExecuteAsync(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task BulkInsertAsync<TEntity>(
        IEnumerable<TEntity> entities,
        string tableName,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        var entityList = entities.ToList();
        if (!entityList.Any())
            return;

        var properties = typeof(TEntity).GetProperties();
        var columnNames = string.Join(", ", properties.Select(p => p.Name));

        var values = entityList.Select((entity, index) =>
        {
            var parameterNames = string.Join(", ", properties.Select(p => $"@param{index}_{p.Name}"));
            return $"({parameterNames})";
        });

        var valuesList = string.Join(", ", values);
        var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES {valuesList}";

        var parameters = new DynamicParameters();
        for (int i = 0; i < entityList.Count; i++)
        {
            var entity = entityList[i];
            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                parameters.Add($"@param{i}_{property.Name}", value);
            }
        }

        await _connection.ExecuteAsync(sql, parameters);
    }

    public async Task BulkUpdateAsync<TEntity>(
        IEnumerable<TEntity> entities,
        string tableName,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        var entityList = entities.ToList();
        if (!entityList.Any())
            return;

        var properties = typeof(TEntity).GetProperties();
        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");

        if (idProperty is null)
            throw new InvalidOperationException("Entity must have an Id property for bulk update");

        foreach (var entity in entityList)
        {
            var setClause = string.Join(", ", properties
                .Where(p => p.Name != "Id")
                .Select(p => $"{p.Name} = @{p.Name}"));

            var sql = $"UPDATE {tableName} SET {setClause} WHERE Id = @Id";

            var parameters = new DynamicParameters();
            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                parameters.Add($"@{property.Name}", value);
            }

            await _connection.ExecuteAsync(sql, parameters);
        }
    }

    public async Task BulkDeleteAsync(
        string tableName,
        string whereClause,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }

        var sql = $"DELETE FROM {tableName} WHERE {whereClause}";
        await _connection.ExecuteAsync(sql, parameters);
    }
}
