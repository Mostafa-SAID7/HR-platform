namespace HR.Attendance.Features.CheckOut;

using HR.Attendance.Application.Dtos.AttendanceRecord;

public record CheckOutCommand(CheckOutRequest Request, Guid TenantId) : ICommand<AttendanceRecordDto>;
