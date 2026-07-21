namespace HR.Audit.Application.Services;

/// <summary>
/// Consumes domain events from Kafka and converts them to audit events
/// Stores in Redis cache (event sourcing - no separate database)
/// </summary>
public class AuditEventConsumer : EventConsumerBase
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuditEventConsumer> _logger;
    private readonly IMediator _mediator;

    public AuditEventConsumer(
        IDistributedCache cache,
        ILogger<AuditEventConsumer> logger,
        IMediator mediator)
    {
        _cache = cache;
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Consume employee events and convert to audit events
    /// </summary>
    public async Task ConsumeEmployeeEventAsync(
        string eventType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditEvent = AuditEvent.FromEvent(
                entityId: entityId,
                entityType: "Employee",
                action: eventType,
                userId: userId,
                userEmail: userEmail,
                oldValues: oldValues,
                newValues: newValues,
                reason: $"Employee {eventType} event from Kafka",
                severity: GetSeverityFromAction(eventType));

            await StoreAuditEventAsync(auditEvent, cancellationToken);
            _logger.LogInformation("Audit event recorded for Employee {EntityId}: {Action}", entityId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming employee event for {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Consume performance events and convert to audit events
    /// </summary>
    public async Task ConsumePerformanceEventAsync(
        string eventType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        try
        {
            var auditEvent = AuditEvent.FromEvent(
                entityId: entityId,
                entityType: "Performance",
                action: eventType,
                userId: userId,
                userEmail: userEmail,
                oldValues: oldValues,
                newValues: newValues,
                reason: $"Performance {eventType} event from Kafka",
                severity: GetSeverityFromAction(eventType));

            await StoreAuditEventAsync(auditEvent, cancellationToken);
            _logger.LogInformation("Audit event recorded for Performance {EntityId}: {Action}", entityId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming performance event for {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Consume payroll events and convert to audit events
    /// </summary>
    public async Task ConsumePayrollEventAsync(
        string eventType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        try
        {
            var severity = eventType switch
            {
                "ProcessPayment" => AuditEventSeverity.Critical,
                "ApprovePayroll" => AuditEventSeverity.Critical,
                _ => AuditEventSeverity.Info
            };

            var auditEvent = AuditEvent.FromEvent(
                entityId: entityId,
                entityType: "Payroll",
                action: eventType,
                userId: userId,
                userEmail: userEmail,
                oldValues: oldValues,
                newValues: newValues,
                reason: $"Payroll {eventType} event from Kafka",
                severity: severity);

            await StoreAuditEventAsync(auditEvent, cancellationToken);
            _logger.LogInformation("Audit event recorded for Payroll {EntityId}: {Action}", entityId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming payroll event for {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Consume recruitment events and convert to audit events
    /// </summary>
    public async Task ConsumeRecruitmentEventAsync(
        string eventType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        try
        {
            var severity = eventType switch
            {
                "OfferExtended" => AuditEventSeverity.Critical,
                "OfferAccepted" => AuditEventSeverity.Critical,
                _ => AuditEventSeverity.Info
            };

            var auditEvent = AuditEvent.FromEvent(
                entityId: entityId,
                entityType: "Recruitment",
                action: eventType,
                userId: userId,
                userEmail: userEmail,
                oldValues: oldValues,
                newValues: newValues,
                reason: $"Recruitment {eventType} event from Kafka",
                severity: severity);

            await StoreAuditEventAsync(auditEvent, cancellationToken);
            _logger.LogInformation("Audit event recorded for Recruitment {EntityId}: {Action}", entityId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming recruitment event for {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Store audit event in Redis cache (event sourcing)
    /// </summary>
    private async Task StoreAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Store individual event
            var eventKey = $"audit:event:{auditEvent.Id}";
            var eventJson = System.Text.Json.JsonSerializer.Serialize(auditEvent);
            await _cache.SetStringAsync(eventKey, eventJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2555) // ~7 years
            }, cancellationToken);

            // Add to entity audit trail
            var trailKey = $"audit:trail:{auditEvent.EntityType}:{auditEvent.EntityId}";
            var trail = await GetAuditTrailAsync(auditEvent.EntityType, auditEvent.EntityId, cancellationToken);
            trail.AddEvent(auditEvent);

            var trailJson = System.Text.Json.JsonSerializer.Serialize(trail);
            await _cache.SetStringAsync(trailKey, trailJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2555)
            }, cancellationToken);

            // Add to critical events index (for compliance)
            if (auditEvent.Severity == AuditEventSeverity.Critical)
            {
                var criticalKey = $"audit:critical:{DateTime.UtcNow:yyyy-MM}";
                var criticalEvents = await GetCriticalEventsAsync(criticalKey, cancellationToken);
                criticalEvents.Add(auditEvent.Id);

                var criticalJson = System.Text.Json.JsonSerializer.Serialize(criticalEvents);
                await _cache.SetStringAsync(criticalKey, criticalJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2555)
                }, cancellationToken);
            }

            _logger.LogInformation("Audit event stored: {EventId}", auditEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing audit event {EventId}", auditEvent.Id);
            throw;
        }
    }

    /// <summary>
    /// Get audit trail for an entity from cache
    /// </summary>
    private async Task<AuditTrail> GetAuditTrailAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var trailKey = $"audit:trail:{entityType}:{entityId}";
            var trailJson = await _cache.GetStringAsync(trailKey, cancellationToken);

            if (!string.IsNullOrEmpty(trailJson))
            {
                return System.Text.Json.JsonSerializer.Deserialize<AuditTrail>(trailJson) ?? CreateNewTrail(entityType, entityId);
            }

            return CreateNewTrail(entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for {EntityType}:{EntityId}", entityType, entityId);
            return CreateNewTrail(entityType, entityId);
        }
    }

    /// <summary>
    /// Get critical events from cache
    /// </summary>
    private async Task<List<Guid>> GetCriticalEventsAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(json))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving critical events from {Key}", key);
            return [];
        }
    }

    /// <summary>
    /// Create a new audit trail
    /// </summary>
    private static AuditTrail CreateNewTrail(string entityType, Guid entityId)
    {
        return new AuditTrail
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Events = [],
            ChangeCount = 0
        };
    }

    /// <summary>
    /// Determine severity from action
    /// </summary>
    private static AuditEventSeverity GetSeverityFromAction(string action)
    {
        return action switch
        {
            "Delete" or "Terminate" or "ProcessPayment" or "ApprovePayroll" => AuditEventSeverity.Critical,
            "Update" or "Approve" or "Reject" => AuditEventSeverity.Warning,
            _ => AuditEventSeverity.Info
        };
    }
}
