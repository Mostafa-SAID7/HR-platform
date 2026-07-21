namespace HR.Analytics.Features.EventConsumers;

using HR.Common.Messaging;
using MassTransit;
using Serilog;

/// <summary>
/// Consumes employee events from Kafka and updates Analytics.
/// </summary>
public class EmployeeEventConsumer : EventConsumerBase<EmployeeCreatedEvent>
{
    private readonly IMediator _mediator;

    public EmployeeEventConsumer(
        IMediator mediator,
        ILogger<EmployeeEventConsumer> logger)
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

/// <summary>
/// Domain event: Employee Created.
/// </summary>
public class EmployeeCreatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Consumes employee updated events.
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

/// <summary>
/// Domain event: Employee Updated.
/// </summary>
public class EmployeeUpdatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Consumes employee deleted events.
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

/// <summary>
/// Domain event: Employee Deleted.
/// </summary>
public class EmployeeDeletedEvent
{
    public Guid EmployeeId { get; set; }
    public DateTime DeletedAt { get; set; }
}
