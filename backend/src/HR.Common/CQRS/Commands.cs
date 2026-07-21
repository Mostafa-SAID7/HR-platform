namespace HR.Common.CQRS;

/// <summary>
/// Base interface for commands (write operations).
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Base interface for commands that return a response.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Base handler for commands that don't return a response.
/// </summary>
public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
    public abstract Task Handle(TCommand request, CancellationToken cancellationToken);
}

/// <summary>
/// Base handler for commands that return a response.
/// </summary>
public abstract class CommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}
