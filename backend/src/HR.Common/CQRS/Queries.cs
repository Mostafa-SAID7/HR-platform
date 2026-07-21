namespace HR.Common.CQRS;

/// <summary>
/// Base interface for queries (read operations).
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Base handler for queries.
/// </summary>
public abstract class QueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public abstract Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
}
