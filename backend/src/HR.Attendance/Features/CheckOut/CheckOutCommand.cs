namespace HR.Attendance.Features.CheckOut;

using HR.Attendance.Application.Dtos;
using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;

public record CheckOutCommand(CheckOutRequest Request, Guid TenantId) : ICommand<AttendanceRecordDto>;

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, AttendanceRecordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AttendanceHub> _hubContext;
    private readonly ILogger<CheckOutCommandHandler> _logger;

    public CheckOutCommandHandler(
        IUnitOfWork unitOfWork,
        IHubContext<AttendanceHub> hubContext,
        ILogger<CheckOutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<AttendanceRecordDto> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var repo = _unitOfWork.GetRepository<AttendanceRecord>();

        var record = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(r => r.EmployeeId == req.EmployeeId && 
                r.AttendanceDate.Date == req.CheckOutTime.Date && 
                r.TenantId == request.TenantId, cancellationToken);

        if (record is null)
            throw new NotFoundException("AttendanceRecord", req.EmployeeId);

        record.CheckOut(req.CheckOutTime, req.Location);
        repo.Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var @event = new RealTimeAttendanceEvent
        {
            EmployeeId = req.EmployeeId,
            EmployeeName = record.EmployeeName,
            EventType = "CheckOut",
            Timestamp = req.CheckOutTime,
            Location = req.Location,
            WorkHours = record.WorkHours
        };

        await _hubContext.Clients.All.SendAsync("ReceiveCheckOut", @event, cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} checked out. Work hours: {WorkHours}", 
            req.EmployeeId, record.WorkHours);

        return MapToDto(record);
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord record)
    {
        return new AttendanceRecordDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = record.EmployeeName,
            AttendanceDate = record.AttendanceDate,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            WorkHours = record.WorkHours,
            Status = record.Status,
            CheckInLocation = record.CheckInLocation,
            CheckOutLocation = record.CheckOutLocation
        };
    }
}
