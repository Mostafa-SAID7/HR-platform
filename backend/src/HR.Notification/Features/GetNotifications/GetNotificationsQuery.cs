namespace HR.Notification.Features.GetNotifications;

using HR.Notification.Application.Dtos.Notification;

/// <summary>
/// Get all notifications for a user
/// </summary>
public record GetNotificationsQuery(
    NotificationFilterDto Filter,
    Guid TenantId) : IQuery<PaginatedResult<NotificationListDto>>;
{
    private readonly NotificationDbContext _dbContext;

    public GetNotificationsQueryHandler(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResult<NotificationListDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications.AsQueryable();

        // Filter by recipient
        if (request.Filter.RecipientId.HasValue && request.Filter.RecipientId != Guid.Empty)
        {
            query = query.Where(x => x.RecipientId == request.Filter.RecipientId.Value);
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(request.Filter.Status))
        {
            if (Enum.TryParse<NotificationStatus>(request.Filter.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
            .Take(request.Filter.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var dtos = notifications.Adapt<List<NotificationListDto>>();

        return new PaginatedResult<NotificationListDto>(
            dtos,
            totalCount,
            request.Filter.Page,
            request.Filter.PageSize);
    }
}
