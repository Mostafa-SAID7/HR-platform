namespace HR.Attendance.Features.GetTodayAttendance;

using HR.Attendance.Application.Dtos.AttendanceRecord;

public record GetTodayAttendanceQuery(Guid EmployeeId, Guid TenantId) : IQuery<AttendanceRecordDto>;
