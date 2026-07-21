namespace HR.Attendance.Application.Mappings;

using HR.Attendance.Domain;
using HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Centralized mapping configuration for AttendanceRecord DTOs.
/// </summary>
public static class AttendanceRecordDtoMappingConfiguration
{
    public static AttendanceRecordDto ToDto(this AttendanceRecord record)
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
