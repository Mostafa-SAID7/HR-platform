namespace HR.Attendance.Features.CheckIn;

/// <summary>
/// Command definition for employee check-in.
/// SOLID: Command separated from validator and handler.
/// </summary>
public record CheckInCommand(CheckInRequest Request, Guid TenantId) : ICommand<AttendanceRecordDto>;
