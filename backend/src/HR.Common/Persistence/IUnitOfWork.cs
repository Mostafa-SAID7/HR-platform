namespace HR.Common.Persistence;

/// <summary>
/// Unit of Work pattern interface for managing database transactions and repositories.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Get or create a repository for the specified entity type.
    /// </summary>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;

    /// <summary>
    /// Save changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
