# Notification Service

**Port**: 5008 | **Status**: ✅ Phase 2 Complete | **Database**: PostgreSQL | **Pattern**: Event Consumer

## Overview

The Notification Service handles all internal and external notifications including email, SMS, and in-app notifications triggered by system events.

## Key Features

- ✅ Multi-channel notifications (Email, SMS, In-App)
- ✅ Event-driven notification triggering
- ✅ Template-based messages
- ✅ Notification preferences
- ✅ Notification history tracking
- ✅ Retry mechanism for failed sends
- ✅ Bulk notification support

## API Endpoints

```
POST   /api/notifications/send
GET    /api/notifications
GET    /api/notifications/{notificationId}
POST   /api/notifications/preferences
GET    /api/notifications/preferences/{userId}
POST   /api/notifications/mark-read
```

## Domain Model

### Notification Entity

```csharp
public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public string Channel { get; set; } // Email, SMS, InApp
    public string Status { get; set; } // Pending, Sent, Failed
    public int RetryCount { get; set; }
    public DateTime? SentAt { get; set; }
    public string ErrorMessage { get; set; }
}
```

### NotificationPreference Entity

```csharp
public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool InAppNotifications { get; set; }
    public bool SendDigest { get; set; }
}
```

## Kafka Topics (Consumed)

| Topic | Event |
|-------|-------|
| `employee.created` | Send welcome email |
| `employee.terminated` | Send goodbye email |
| `performance.review.submitted` | Notify approver |
| `performance.review.approved` | Notify employee |
| `leave.approved` | Notify employee |
| `leave.rejected` | Notify employee |
| `payroll.paid` | Send payment notification |

## Integration Examples

### Send Notification

```bash
curl -X POST http://localhost:5008/api/notifications/send \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "recipientId": "user-guid",
    "subject": "Welcome",
    "message": "Welcome to HR Platform",
    "channel": "Email"
  }'
```

### Set Preferences

```bash
curl -X POST http://localhost:5008/api/notifications/preferences \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "emailNotifications": true,
    "smsNotifications": false,
    "inAppNotifications": true
  }'
```

## Template Examples

### Employee Created

```
Subject: Welcome to {{CompanyName}}!
Message: Dear {{FirstName}},

Welcome to the HR Platform. Your account has been created.
Your username: {{Email}}

Please log in here: {{PortalUrl}}
```

### Leave Approved

```
Subject: Your leave request has been approved
Message: Dear {{EmployeeName}},

Your leave request from {{StartDate}} to {{EndDate}} has been approved.

Thank you,
HR Team
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Notification/
dotnet test tests/HR.Tests.Integration/Notification/
```

## Dependencies

- SendGrid (email)
- Twilio (SMS)
- Kafkaflow (event consumption)
- Entity Framework Core

## Related Services

- [Employee Service](02-EMPLOYEE-SERVICE.md)
- [Performance Service](03-PERFORMANCE-SERVICE.md)
- [Attendance Service](05-ATTENDANCE-SERVICE.md)
