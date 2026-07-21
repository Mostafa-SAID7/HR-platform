namespace HR.Attendance.Features.RequestLeave;

using MediatR;
using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;
using HR.Attendance.Application.Dtos.LeaveRequest;
using HR.Attendance.Application.Mappings;

public class RequestLeaveCommandHandler : IRequestHandler<RequestLeaveCommand, LeaveRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AttendanceHub> _hubContext;
    private readonly ILogger<RequestLeaveCommandHandler> _logger;

    public RequestLeaveCommandHandler(
        IUnitOfWork unitOfWork,
        IHubContext<AttendanceHub> hubContext,
        ILogger<RequestLeaveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<LeaveRequestDto> Handle(RequestLeaveCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var leaveRequest = LeaveRequest.Create(
            req.EmployeeId,
            req.EmployeeName,
            req.LeaveType,
            req.StartDate,
            req.EndDate,
            req.Reason,
            request.TenantId);

        var repo = _unitOfWork.GetRepository<LeaveRequest>();
        await repo.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Use centralized mapping instead of inline
        var dto = leaveRequest.ToDto();

        // Broadcast via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveLeaveRequest", dto, cancellationToken);

        _logger.LogInformation("Leave request created for employee {EmployeeId}, type: {LeaveType}", 
            req.EmployeeId, req.LeaveType);

        return dto;
    }
}
