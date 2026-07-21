namespace HR.Attendance.Features.GetLeaveRequests;

using MediatR;
using HR.Attendance.Application.Dtos.LeaveRequest;
using HR.Attendance.Application.Mappings;

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetLeaveRequestsQueryHandler> _logger;

    public GetLeaveRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetLeaveRequestsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<LeaveRequestDto>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<LeaveRequest>();

        var query = repo.GetAsQueryable()
            .Where(lr => lr.TenantId == request.TenantId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(lr => lr.Status == request.Status);
        }

        var leaveRequests = await query
            .OrderByDescending(lr => lr.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} leave requests for tenant {TenantId}", 
            leaveRequests.Count, request.TenantId);

        // Use centralized mapping instead of inline
        return leaveRequests.Select(lr => lr.ToDto()).ToList();
    }
}
