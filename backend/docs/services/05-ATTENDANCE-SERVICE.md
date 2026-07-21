# Attendance Service

**Port**: 5005 | **Status**: ✅ Production Ready | **Database**: PostgreSQL | **Real-time**: SignalR

## Overview

The Attendance Service tracks employee attendance, check-in/check-out times, leave requests, and work hour calculations with real-time updates via SignalR.

## Key Features

- ✅ Real-time check-in/check-out
- ✅ Work hours calculation
- ✅ Leave request management
- ✅ Automatic leave marking
- ✅ Late arrival tracking
- ✅ Attendance reports
- ✅ SignalR real-time notifications

## API Endpoints

```
POST   /api/attendance/check-in
POST   /api/attendance/check-out
GET    /api/attendance/{employeeId}/today
GET    /api/attendance/{employeeId}/range
POST   /api/leave-requests
GET    /api/leave-requests
POST   /api/leave-requests/{requestId}/approve
POST   /api/leave-requests/{requestId}/reject
GET    /api/attendance/report
```

## Domain Model

### AttendanceRecord

```csharp
public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal WorkHours { get; set; }
    public string Status { get; set; } // Present, Absent, Late, Leave
    public string LeaveType { get; set; }
}
```

### LeaveRequest

```csharp
public class LeaveRequest
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int NumberOfDays { get; set; }
    public string LeaveType { get; set; } // Vacation, Sick, Unpaid
    public string Status { get; set; } // Pending, Approved, Rejected
    public string RejectionReason { get; set; }
}
```

## Database Schema

### AttendanceRecords Table

| Column | Type |
|--------|------|
| Id | UUID |
| EmployeeId | UUID |
| AttendanceDate | DATE |
| CheckInTime | TIMESTAMP |
| CheckOutTime | TIMESTAMP |
| WorkHours | DECIMAL(5,2) |
| Status | VARCHAR(50) |
| LeaveType | VARCHAR(50) |
| TenantId | UUID |

### LeaveRequests Table

| Column | Type |
|--------|------|
| Id | UUID |
| EmployeeId | UUID |
| StartDate | DATE |
| EndDate | DATE |
| NumberOfDays | INT |
| LeaveType | VARCHAR(50) |
| Status | VARCHAR(50) |
| RejectionReason | TEXT |
| TenantId | UUID |

## Kafka Topics

| Topic | Event |
|-------|-------|
| `attendance.checkin` | Employee checked in |
| `attendance.checkout` | Employee checked out |
| `leave.requested` | Leave request submitted |
| `leave.approved` | Leave approved |
| `leave.rejected` | Leave rejected |

## Integration Examples

### Check-In

```bash
curl -X POST http://localhost:5005/api/attendance/check-in \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": "emp-guid"
  }'
```

### Request Leave

```bash
curl -X POST http://localhost:5005/api/leave-requests \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": "emp-guid",
    "startDate": "2026-08-10",
    "endDate": "2026-08-15",
    "leaveType": "Vacation"
  }'
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Attendance/
dotnet test tests/HR.Tests.Integration/Attendance/
```

## Related Services

- [Employee Service](02-EMPLOYEE-SERVICE.md)
- [Payroll Service](06-PAYROLL-SERVICE.md) - For leave deductions
