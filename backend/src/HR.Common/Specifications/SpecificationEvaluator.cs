namespace HR.Common.Specifications;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Evaluator for applying specifications to EF Core queries.
/// </summary>
public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Apply a specification to a query.
    /// </summary>
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, Specification<TEntity> specification)
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes for eager loading
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip != 0)
            {
                query = query.Skip(specification.Skip);
            }

            if (specification.Take != 0)
            {
                query = query.Take(specification.Take);
            }
        }

        return query;
    }

    /// <summary>
    /// Apply a specification with projection to a query.
    /// </summary>
    public static IQueryable<TResult> GetQuery<TResult>(IQueryable<TEntity> inputQuery, Specification<TEntity, TResult> specification)
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes for eager loading
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip != 0)
            {
                query = query.Skip(specification.Skip);
            }

            if (specification.Take != 0)
            {
                query = query.Take(specification.Take);
            }
        }

        // Apply selector (projection)
        if (specification.Selector is not null)
        {
            return query.Select(specification.Selector);
        }

        return query.Cast<TResult>();
    }
}
