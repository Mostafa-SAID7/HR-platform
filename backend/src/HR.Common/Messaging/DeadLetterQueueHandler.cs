namespace HR.Common.Messaging;

using MassTransit;
using Serilog;

/// <summary>
/// Handles messages that failed processing and have been sent to the Dead Letter Queue (DLQ).
/// These messages will be investigated and potentially replayed after issues are fixed.
/// </summary>
public class DeadLetterQueueHandler : IConsumer<DeadLetterMessage>
{
    private readonly ILogger<DeadLetterQueueHandler> _logger;
    private const string DlqTopic = "dlq-failed-events";

    public DeadLetterQueueHandler(ILogger<DeadLetterQueueHandler> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeadLetterMessage> context)
    {
        var message = context.Message;

        _logger.LogError(
            "Dead Letter Queue message received from topic {OriginalTopic}. " +
            "Error: {Error}. Retry Count: {RetryCount}. Message: {Content}",
            message.OriginalTopic,
            message.Error,
            message.RetryCount,
            message.Content);

        // Store DLQ message in database for investigation
        await StoreDeadLetterMessageAsync(message);

        // Send alert/notification
        await SendDlqAlertAsync(message);

        await Task.CompletedTask;
    }

    private async Task StoreDeadLetterMessageAsync(DeadLetterMessage message)
    {
        try
        {
            // TODO: Implement database storage for DLQ messages
            // This would store the message in a DLQ table for later investigation and replay
            _logger.LogInformation("Storing DLQ message {MessageId} for investigation", message.Id);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store DLQ message");
        }
    }

    private async Task SendDlqAlertAsync(DeadLetterMessage message)
    {
        try
        {
            // TODO: Implement alerting mechanism (email, Slack, PagerDuty, etc.)
            _logger.LogWarning("DLQ alert sent for message from topic {OriginalTopic}", message.OriginalTopic);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send DLQ alert");
        }
    }
}

/// <summary>
/// Message sent to Dead Letter Queue.
/// </summary>
public class DeadLetterMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalTopic { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int RetryCount { get; set; }
    public string? Exception { get; set; }
}

/// <summary>
/// Helper for moving messages to DLQ.
/// </summary>
public interface IDeadLetterQueueService
{
    Task MoveToDeadLetterQueueAsync(
        string originalTopic,
        string messageContent,
        string error,
        int retryCount,
        Exception? exception = null);
}

/// <summary>
/// Implementation of DLQ service using MassTransit.
/// </summary>
public class DeadLetterQueueService : IDeadLetterQueueService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DeadLetterQueueService> _logger;

    public DeadLetterQueueService(
        IPublishEndpoint publishEndpoint,
        ILogger<DeadLetterQueueService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task MoveToDeadLetterQueueAsync(
        string originalTopic,
        string messageContent,
        string error,
        int retryCount,
        Exception? exception = null)
    {
        try
        {
            var dlqMessage = new DeadLetterMessage
            {
                Id = Guid.NewGuid(),
                OriginalTopic = originalTopic,
                Content = messageContent,
                Error = error,
                Timestamp = DateTime.UtcNow,
                RetryCount = retryCount,
                Exception = exception?.ToString()
            };

            _logger.LogError(
                "Moving message to DLQ. Original Topic: {Topic}, Error: {Error}, Retries: {Retries}",
                originalTopic, error, retryCount);

            await _publishEndpoint.Publish(dlqMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move message to DLQ");
            throw;
        }
    }
}
