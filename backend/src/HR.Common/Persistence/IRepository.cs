namespace HR.Common.Persistence;

/// <summary>
/// Generic repository interface for data access patterns.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Get an entity by its ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities (paginated).
    /// </summary>
    Task<PaginatedResult<TEntity>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities as queryable (for LINQ queries).
    /// </summary>
    IQueryable<TEntity> GetAsQueryable();

    /// <summary>
    /// Add a new entity.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Update multiple entities.
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Delete an entity (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, Guid? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently delete an entity (hard delete).
    /// </summary>
    Task DeletePermanentlyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total count of entities.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
