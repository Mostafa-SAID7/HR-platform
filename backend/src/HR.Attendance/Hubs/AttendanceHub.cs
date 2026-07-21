namespace HR.Attendance.Hubs;

using Microsoft.AspNetCore.SignalR;
using HR.Attendance.Application.Dtos;
using Serilog;

/// <summary>
/// SignalR hub for real-time attendance updates.
/// </summary>
public class AttendanceHub : Hub
{
    private readonly ILogger<AttendanceHub> _logger;

    public AttendanceHub(ILogger<AttendanceHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to AttendanceHub: {ConnectionId}", Context.ConnectionId);
        await Clients.Caller.SendAsync("ReceiveMessage", "Connected to Attendance Service");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from AttendanceHub: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a group for real-time updates (e.g., department-wide view).
    /// </summary>
    public async Task JoinDepartmentGroup(string departmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        _logger.LogInformation("Client {ConnectionId} joined department group: {DepartmentId}", 
            Context.ConnectionId, departmentId);
    }

    /// <summary>
    /// Leave a department group.
    /// </summary>
    public async Task LeaveDepartmentGroup(string departmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        _logger.LogInformation("Client {ConnectionId} left department group: {DepartmentId}", 
            Context.ConnectionId, departmentId);
    }

    /// <summary>
    /// Broadcast real-time check-in event to all connected clients.
    /// </summary>
    public async Task BroadcastCheckIn(RealTimeAttendanceEvent @event, string? departmentId = null)
    {
        if (!string.IsNullOrEmpty(departmentId))
        {
            await Clients.Group($"department-{departmentId}").SendAsync("ReceiveCheckIn", @event);
        }
        else
        {
            await Clients.All.SendAsync("ReceiveCheckIn", @event);
        }

        _logger.LogInformation("Broadcasted check-in event for employee {EmployeeId}", @event.EmployeeId);
    }

    /// <summary>
    /// Broadcast real-time check-out event to all connected clients.
    /// </summary>
    public async Task BroadcastCheckOut(RealTimeAttendanceEvent @event, string? departmentId = null)
    {
        if (!string.IsNullOrEmpty(departmentId))
        {
            await Clients.Group($"department-{departmentId}").SendAsync("ReceiveCheckOut", @event);
        }
        else
        {
            await Clients.All.SendAsync("ReceiveCheckOut", @event);
        }

        _logger.LogInformation("Broadcasted check-out event for employee {EmployeeId}", @event.EmployeeId);
    }

    /// <summary>
    /// Broadcast leave request notification.
    /// </summary>
    public async Task BroadcastLeaveRequest(LeaveRequestDto leaveRequest)
    {
        await Clients.All.SendAsync("ReceiveLeaveRequest", leaveRequest);
        _logger.LogInformation("Broadcasted leave request for employee {EmployeeId}", leaveRequest.EmployeeId);
    }

    /// <summary>
    /// Broadcast attendance summary update.
    /// </summary>
    public async Task SendAttendanceSummary(Guid employeeId, AttendanceSummaryDto summary)
    {
        await Clients.User(employeeId.ToString()).SendAsync("ReceiveAttendanceSummary", summary);
        _logger.LogInformation("Sent attendance summary to employee {EmployeeId}", employeeId);
    }
}
