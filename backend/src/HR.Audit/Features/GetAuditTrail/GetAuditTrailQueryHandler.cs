namespace HR.Audit.Features.GetAuditTrail;

using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using HR.Audit.Application.Mappings;

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
                
                // Return empty trail using centralized mapping
                var emptyTrail = AuditTrail.CreateEmpty(request.EntityId, request.EntityType, request.TenantId);
                return emptyTrail.ToDetailDto();
            }

            var trail = System.Text.Json.JsonSerializer.Deserialize<AuditTrail>(trailJson);
            if (trail == null)
                throw new InvalidOperationException("Failed to deserialize audit trail");

            // Use centralized mapping instead of inline
            return trail.ToDetailDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for {EntityType}:{EntityId}", request.EntityType, request.EntityId);
            throw new DomainException($"Error retrieving audit trail: {ex.Message}");
        }
    }
}
