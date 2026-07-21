namespace HR.Analytics.Features.EventConsumers;

using HR.Common.Messaging;
using MassTransit;
using HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Consumes employee created events from Kafka and updates Analytics.
/// SOLID: Consumer separated from events and other consumers.
/// </summary>
public class EmployeeCreatedEventConsumer : EventConsumerBase<EmployeeCreatedEvent>
{
    private readonly IMediator _mediator;

    public EmployeeCreatedEventConsumer(
        IMediator mediator,
        ILogger<EmployeeCreatedEventConsumer> logger)
        : base(logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;

        Logger.LogInformation("Updating analytics for newly created employee {EmployeeId}", @event.EmployeeId);

        // TODO: Implement update analytics logic
        // Example:
        // var command = new UpdateEmployeeAnalyticsCommand(@event.EmployeeId, @event.EmployeeName);
        // await _mediator.Send(command);

        await Task.CompletedTask;
    }
}
