namespace HR.Audit.Features.GetAuditTrail;

/// <summary>
/// Get audit trail for an entity
/// </summary>
public record GetAuditTrailQuery(
    Guid EntityId,
    string EntityType,
    Guid TenantId) : IQuery<AuditTrailDetailDto>;

/// <summary>
/// Handler for GetAuditTrailQuery
/// </summary>
public class GetAuditTrailQueryHandler : IQueryHandler<GetAuditTrailQuery, AuditTrailDetailDto>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetAuditTrailQueryHandler> _logger;

    public GetAuditTrailQueryHandler(
        IDistributedCache cache,
        ILogger<GetAuditTrailQueryHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<AuditTrailDetailDto> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var trailKey = $"audit:trail:{request.EntityType}:{request.EntityId}";
            var trailJson = await _cache.GetStringAsync(trailKey, cancellationToken);

            if (string.IsNullOrEmpty(trailJson))
            {
                _logger.LogWarning("No audit trail found for {EntityType}:{EntityId}", request.EntityType, request.EntityId);
                
                // Return empty trail
                return new AuditTrailDetailDto(
                    request.EntityId,
                    request.EntityType,
                    DateTime.MinValue,
                    DateTime.MinValue,
                    0,
                    [],
                    []);
            }

            var trail = System.Text.Json.JsonSerializer.Deserialize<AuditTrail>(trailJson);
            if (trail == null)
                throw new InvalidOperationException("Failed to deserialize audit trail");

            var eventDtos = trail.Events
                .Select(e => new AuditEventDto(
                    e.Id,
                    e.EntityId,
                    e.EntityType,
                    e.Action,
                    e.UserId,
                    e.UserEmail,
                    e.Timestamp,
                    e.OldValues,
                    e.NewValues,
                    e.Reason,
                    e.Severity.ToString(),
                    e.IpAddress))
                .ToList();

            return new AuditTrailDetailDto(
                trail.EntityId,
                trail.EntityType,
                trail.FirstChangeAt,
                trail.LastChangeAt,
                trail.ChangeCount,
                trail.AffectedUsers,
                eventDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for {EntityType}:{EntityId}", request.EntityType, request.EntityId);
            throw new DomainException($"Error retrieving audit trail: {ex.Message}");
        }
    }
}
