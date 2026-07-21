namespace HR.Common.Messaging;

using MassTransit;
using Serilog;

/// <summary>
/// Base class for event consumers with built-in error handling and logging.
/// </summary>
public abstract class EventConsumerBase<TEvent> : IConsumer<TEvent>
    where TEvent : class
{
    protected readonly ILogger<EventConsumerBase<TEvent>> Logger;

    protected EventConsumerBase(ILogger<EventConsumerBase<TEvent>> logger)
    {
        Logger = logger;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            var eventType = typeof(TEvent).Name;
            Logger.LogInformation("Consuming event {EventType} with correlationId {CorrelationId}",
                eventType, context.CorrelationId);

            await HandleAsync(context);

            Logger.LogInformation("Event {EventType} consumed successfully", eventType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error consuming event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }

    /// <summary>
    /// Override this method to implement event handling logic.
    /// </summary>
    protected abstract Task HandleAsync(ConsumeContext<TEvent> context);
}
