namespace HR.Common.Outbox;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for managing outbox messages and publishing events.
/// </summary>
public class OutboxService : IOutboxService
{
    private readonly DbContext _context;
    private readonly ICacheService _cacheService;

    public OutboxService(DbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task AddMessageAsync(
        DomainEvent domainEvent,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = domainEvent.GetType().FullName ?? string.Empty,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
            ServiceName = serviceName,
            TenantId = domainEvent.TenantId,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            IsDeleted = false
        };

        _context.Add(outboxMessage);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMessagesAsync(
        IEnumerable<DomainEvent> domainEvents,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        var outboxMessages = domainEvents.Select(de => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = de.GetType().FullName ?? string.Empty,
            Content = JsonSerializer.Serialize(de),
            OccurredOnUtc = de.OccurredOnUtc,
            ServiceName = serviceName,
            TenantId = de.TenantId,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            IsDeleted = false
        }).ToList();

        _context.AddRange(outboxMessages);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        var messages = await _context.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed && !m.IsDeleted)
            .OrderBy(m => m.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task MarkAsProcessedAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is not null)
        {
            message.ProcessedOnUtc = DateTime.UtcNow;
            message.ErrorMessage = null;
            _context.Update(message);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache for this message
            await _cacheService.RemoveAsync($"outbox:unprocessed", cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(
        Guid messageId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is not null)
        {
            message.RetryCount++;
            message.ErrorMessage = errorMessage;
            _context.Update(message);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache for this message
            await _cacheService.RemoveAsync($"outbox:unprocessed", cancellationToken);
        }
    }

    public async Task<IEnumerable<OutboxMessage>> GetDeadLetterMessagesAsync(
        int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        var deadLetterMessages = await _context.Set<OutboxMessage>()
            .Where(m => m.RetryCount >= maxRetries && !m.IsDeleted)
            .OrderBy(m => m.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        return deadLetterMessages;
    }
}
