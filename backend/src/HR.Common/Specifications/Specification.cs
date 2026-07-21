namespace HR.Common.Specifications;

using System.Linq.Expressions;

/// <summary>
/// Base class for specifications - a pattern for encapsulating query logic.
/// </summary>
public abstract class Specification<TEntity> where TEntity : BaseEntity
{
    public Expression<Func<TEntity, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<TEntity, object>>? OrderBy { get; protected set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; } = false;

    /// <summary>
    /// Add an include expression for eager loading.
    /// </summary>
    protected virtual void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add a string-based include for complex navigation properties.
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Set pagination.
    /// </summary>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Set ordering ascending.
    /// </summary>
    protected virtual void ApplyOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Set ordering descending.
    /// </summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
}

/// <summary>
/// Base class for specifications with a single result type.
/// </summary>
public abstract class Specification<TEntity, TResult> : Specification<TEntity> where TEntity : BaseEntity
{
    public Expression<Func<TEntity, TResult>>? Selector { get; protected set; }

    /// <summary>
    /// Set a selector for projection to a different type.
    /// </summary>
    protected virtual void ApplySelector(Expression<Func<TEntity, TResult>> selectorExpression)
    {
        Selector = selectorExpression;
    }
}
