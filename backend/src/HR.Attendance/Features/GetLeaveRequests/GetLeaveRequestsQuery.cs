namespace HR.Attendance.Features.GetLeaveRequests;

public record GetLeaveRequestsQuery(Guid TenantId, string? Status = null) : IQuery<List<LeaveRequestDto>>;

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

        return leaveRequests.Select(MapToDto).ToList();
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
    {
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = leaveRequest.EmployeeName,
            LeaveType = leaveRequest.LeaveType,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            LeaveDays = leaveRequest.LeaveDays,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedDate = leaveRequest.ApprovedDate
        };
    }
}
