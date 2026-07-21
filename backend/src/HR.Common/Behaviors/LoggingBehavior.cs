namespace HR.Common.Behaviors;

using Serilog;

/// <summary>
/// MediatR pipeline behavior for automatic logging.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Guid.NewGuid();

        Log.Information(
            "Handling {@RequestName} with correlation ID {@CorrelationId} with {@Request}",
            requestName,
            correlationId,
            request);

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();

            var duration = DateTime.UtcNow - startTime;

            Log.Information(
                "Handled {@RequestName} with correlation ID {@CorrelationId} in {@Duration}ms",
                requestName,
                correlationId,
                duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            Log.Error(
                ex,
                "Error handling {@RequestName} with correlation ID {@CorrelationId} after {@Duration}ms",
                requestName,
                correlationId,
                duration.TotalMilliseconds);

            throw;
        }
    }
}
