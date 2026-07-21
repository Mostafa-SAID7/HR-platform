namespace HR.Analytics.Features.EventConsumers;

using HR.Common.Messaging;
using MassTransit;
using HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Consumes employee updated events from Kafka and updates Analytics.
/// SOLID: Consumer separated from events and other consumers.
/// </summary>
public class EmployeeUpdatedEventConsumer : EventConsumerBase<EmployeeUpdatedEvent>
{
    private readonly IMediator _mediator;

    public EmployeeUpdatedEventConsumer(
        IMediator mediator,
        ILogger<EmployeeUpdatedEventConsumer> logger)
        : base(logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(ConsumeContext<EmployeeUpdatedEvent> context)
    {
        var @event = context.Message;

        Logger.LogInformation("Updating analytics for updated employee {EmployeeId}", @event.EmployeeId);

        // TODO: Implement update analytics logic

        await Task.CompletedTask;
    }
}
