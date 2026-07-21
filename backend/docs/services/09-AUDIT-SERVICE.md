# Audit Service

**Port**: 5009 | **Status**: ✅ Phase 2 Complete | **Database**: PostgreSQL | **Pattern**: Event Consumer

## Overview

The Audit Service provides comprehensive audit logging and compliance tracking for all critical operations in the HR platform, capturing who did what, when, and why.

## Key Features

- ✅ Centralized audit logging
- ✅ Event-driven audit trail
- ✅ User action tracking
- ✅ Change history preservation
- ✅ Compliance reporting
- ✅ Audit trail immutability
- ✅ Multi-level logging (Create, Update, Delete, Approve)

## API Endpoints

```
GET    /api/audit/events
GET    /api/audit/events/{entityId}
GET    /api/audit/users/{userId}
GET    /api/audit/report
GET    /api/audit/changes/{entityId}
```

## Domain Model

### AuditEvent Entity

```csharp
public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string EntityType { get; set; } // Employee, Payroll, Leave, etc.
    public Guid EntityId { get; set; }
    public string Action { get; set; } // Create, Update, Delete, Approve
    public string OldValues { get; set; } // JSON
    public string NewValues { get; set; } // JSON
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}
```

## Kafka Topics (Consumed)

All system events are captured:

| Topic | Logged As |
|-------|-----------|
| `employee.*` | Employee changes |
| `payroll.*` | Payroll changes |
| `performance.*` | Performance changes |
| `leave.*` | Leave changes |
| `identity.user.*` | User authentication |
| `attendance.*` | Attendance changes |

## Integration Examples

### Get Audit Trail for Entity

```bash
curl "http://localhost:5009/api/audit/events?entityId=emp-guid&entityType=Employee" \
  -H "Authorization: Bearer token"

# Response
[
  {
    "id": "audit-guid",
    "userId": "user-guid",
    "userName": "admin@example.com",
    "action": "Update",
    "timestamp": "2026-07-21T10:30:00Z",
    "oldValues": {"salary": 100000},
    "newValues": {"salary": 120000}
  }
]
```

### Get User Activity

```bash
curl "http://localhost:5009/api/audit/users/{userId}" \
  -H "Authorization: Bearer token"
```

### Generate Compliance Report

```bash
curl "http://localhost:5009/api/audit/report?startDate=2026-01-01&endDate=2026-07-31" \
  -H "Authorization: Bearer token"
```

## Audit Event Example

```json
{
  "id": "audit-12345",
  "userId": "user-99",
  "userName": "manager@example.com",
  "entityType": "Employee",
  "entityId": "emp-54321",
  "action": "Approved",
  "oldValues": {
    "salary": "100000",
    "jobTitle": "Engineer"
  },
  "newValues": {
    "salary": "120000",
    "jobTitle": "Senior Engineer"
  },
  "timestamp": "2026-07-21T15:45:30Z",
  "ipAddress": "192.168.1.100"
}
```

## Compliance Reports

### Employee Data Access Report

```bash
curl "http://localhost:5009/api/audit/report?type=DataAccess&entityType=Employee" \
  -H "Authorization: Bearer token"
```

### Payroll Changes Report

```bash
curl "http://localhost:5009/api/audit/report?type=PayrollChanges&month=7&year=2026" \
  -H "Authorization: Bearer token"
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Audit/
dotnet test tests/HR.Tests.Integration/Audit/
```

## Dependencies

- Entity Framework Core 9.0
- Kafkaflow (event consumption)
- Serilog (structured logging)

## Related Services

- All services (event sources)
