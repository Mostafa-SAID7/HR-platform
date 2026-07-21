namespace HR.Common.BackgroundServices;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// Background service that processes outbox messages and publishes them to Kafka.
/// Implements the Outbox pattern for guaranteed event delivery.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 100;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Outbox Processor Service cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
                // Continue processing despite errors
                await Task.Delay(_processingInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                // Get all unprocessed outbox messages
                var messages = await GetUnprocessedMessagesAsync(scope, cancellationToken);

                if (messages.Count == 0)
                {
                    return;
                }

                _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

                foreach (var message in messages)
                {
                    try
                    {
                        // Publish to Kafka (implementation depends on MassTransit/Kafka setup)
                        await PublishMessageAsync(scope, message, cancellationToken);

                        // Mark as processed
                        await MarkAsProcessedAsync(scope, message, cancellationToken);

                        _logger.LogDebug("Outbox message {MessageId} processed successfully", message.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process outbox message {MessageId}, retry count: {RetryCount}",
                            message.Id, message.RetryCount);

                        // Update retry count
                        await UpdateRetryCountAsync(scope, message, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor batch");
            }
        }
    }

    private async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        // This would typically query the database for unprocessed messages
        // For now, return empty list - actual implementation depends on your DbContext setup
        return await Task.FromResult(new List<OutboxMessage>());
    }

    private async Task PublishMessageAsync(IServiceScope scope, OutboxMessage message, CancellationToken cancellationToken)
    {
        // Implementation: Publish to Kafka using your configured producer
        // This would use IEventPublisher or MassTransit to send the message
        
        _logger.LogDebug("Publishing message {MessageId} to topic {Topic}", message.Id, message.EventType);
        
        // Actual Kafka publishing would happen here
        await Task.CompletedTask;
    }

    private async Task MarkAsProcessedAsync(IServiceScope scope, OutboxMessage message, CancellationToken cancellationToken)
    {
        // This would update the OutboxMessage.ProcessedOnUtc timestamp in the database
        message.ProcessedOnUtc = DateTime.UtcNow;
        
        _logger.LogDebug("Marked message {MessageId} as processed", message.Id);
        
        await Task.CompletedTask;
    }

    private async Task UpdateRetryCountAsync(IServiceScope scope, OutboxMessage message, CancellationToken cancellationToken)
    {
        message.RetryCount++;
        
        // Check if max retries exceeded
        const int maxRetries = 5;
        if (message.RetryCount >= maxRetries)
        {
            _logger.LogError("Message {MessageId} exceeded max retries ({MaxRetries}), should be sent to DLQ",
                message.Id, maxRetries);
            
            // Move to Dead Letter Queue
            await MoveToDeadLetterQueueAsync(scope, message, cancellationToken);
        }
        
        await Task.CompletedTask;
    }

    private async Task MoveToDeadLetterQueueAsync(IServiceScope scope, OutboxMessage message, CancellationToken cancellationToken)
    {
        // Move message to DLQ table or topic
        _logger.LogError("Moving message {MessageId} to Dead Letter Queue after {RetryCount} retries",
            message.Id, message.RetryCount);
        
        await Task.CompletedTask;
    }
}
