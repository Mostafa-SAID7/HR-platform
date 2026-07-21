namespace HR.Analytics.Features.EventConsumers;

using HR.Common.Messaging;
using MassTransit;
using HR.Analytics.Features.EventConsumers.Events;

/// <summary>
/// Consumes employee deleted events from Kafka and updates Analytics.
/// SOLID: Consumer separated from events and other consumers.
/// </summary>
public class EmployeeDeletedEventConsumer : EventConsumerBase<EmployeeDeletedEvent>
{
    private readonly IMediator _mediator;

    public EmployeeDeletedEventConsumer(
        IMediator mediator,
        ILogger<EmployeeDeletedEventConsumer> logger)
        : base(logger)
    {
        _mediator = mediator;
    }

    protected override async Task HandleAsync(ConsumeContext<EmployeeDeletedEvent> context)
    {
        var @event = context.Message;

        Logger.LogInformation("Removing analytics for deleted employee {EmployeeId}", @event.EmployeeId);

        // TODO: Implement delete analytics logic

        await Task.CompletedTask;
    }
}
