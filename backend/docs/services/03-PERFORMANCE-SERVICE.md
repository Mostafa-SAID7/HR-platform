# Performance Service

**Port**: 5003 | **Status**: ✅ Production Ready | **Database**: PostgreSQL | **Pattern**: CQRS + Domain Events

## Overview

The Performance Service manages employee performance reviews, feedback collection, ratings, and performance tracking throughout the review cycle.

## Key Features

- ✅ Performance review lifecycle (Draft → Submitted → Approved)
- ✅ Multi-category feedback collection
- ✅ Rating system (1-5 scale with descriptions)
- ✅ Approval workflow with notifications
- ✅ Historical review tracking
- ✅ Performance trends and analytics
- ✅ Domain events for external systems

## API Endpoints

```
POST   /api/performance-reviews
GET    /api/performance-reviews
GET    /api/performance-reviews/{reviewId}
PUT    /api/performance-reviews/{reviewId}
POST   /api/performance-reviews/{reviewId}/submit
POST   /api/performance-reviews/{reviewId}/approve
POST   /api/performance-reviews/{reviewId}/reject
POST   /api/performance-reviews/{reviewId}/feedback
GET    /api/performance-reviews/employee/{employeeId}
```

## Domain Model

### PerformanceReview Aggregate

```csharp
public class PerformanceReview
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string ReviewPeriod { get; set; } // "Q3 2026"
    public DateTime ReviewDate { get; set; }
    public string Status { get; set; } // Draft, Submitted, Approved, Rejected
    public string OverallRating { get; set; } // "5 - Exceeds", "4 - Meets", etc.
    public string RejectionReason { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public List<PerformanceFeedback> Feedback { get; set; }
    public List<DomainEvent> DomainEvents { get; set; }
}
```

### PerformanceFeedback Entity

```csharp
public class PerformanceFeedback
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public string Category { get; set; } // Communication, Technical Skills, Leadership
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Database Schema

### PerformanceReviews Table

| Column | Type | Description |
|--------|------|-------------|
| Id | UUID | Primary key |
| EmployeeId | UUID | Employee reference |
| ReviewPeriod | VARCHAR(50) | e.g., "Q3 2026" |
| ReviewDate | DATE | Review date |
| Status | VARCHAR(50) | Draft, Submitted, Approved, Rejected |
| OverallRating | VARCHAR(256) | Rating description |
| RejectionReason | TEXT | Reason if rejected |
| SubmittedDate | TIMESTAMP | Submission time |
| ApprovedDate | TIMESTAMP | Approval time |
| TenantId | UUID | Tenant ID |

### PerformanceFeedback Table

| Column | Type |
|--------|------|
| Id | UUID |
| ReviewId | UUID |
| Category | VARCHAR(256) |
| Comment | TEXT |
| CreatedAt | TIMESTAMP |

## Rating Scale

| Rating | Description |
|--------|-------------|
| 5 | Exceeds Expectations |
| 4 | Meets Expectations |
| 3 | Meets Some Expectations |
| 2 | Below Expectations |
| 1 | Far Below Expectations |

## Kafka Topics

| Topic | Event | Schema |
|-------|-------|--------|
| `performance.review.submitted` | Review submitted | ReviewId, EmployeeId, SubmittedAt |
| `performance.review.approved` | Review approved | ReviewId, EmployeeId, Rating, ApprovedAt |
| `performance.review.rejected` | Review rejected | ReviewId, EmployeeId, Reason |

## Integration Examples

### Create Performance Review

```bash
curl -X POST http://localhost:5003/api/performance-reviews \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": "emp-guid",
    "reviewPeriod": "Q3 2026",
    "reviewDate": "2026-09-30"
  }'
```

### Add Feedback

```bash
curl -X POST http://localhost:5003/api/performance-reviews/{reviewId}/feedback \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "category": "Technical Skills",
    "comment": "Excellent problem-solving and coding practices"
  }'
```

### Submit Review

```bash
curl -X POST http://localhost:5003/api/performance-reviews/{reviewId}/submit \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "overallRating": "4 - Meets Expectations"
  }'
```

### Approve Review

```bash
curl -X POST http://localhost:5003/api/performance-reviews/{reviewId}/approve \
  -H "Authorization: Bearer token"
```

## Workflow

```
Draft → (Add Feedback & Rating) → Submit → Approve
                                    ↓
                              (Send Notification)
                              Reject (with reason)
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Performance/
dotnet test tests/HR.Tests.Integration/Performance/
```

## Dependencies

- Entity Framework Core 9.0
- Kafkaflow
- MassTransit (optional, for advanced workflow)

## Related Services

- [Employee Service](02-EMPLOYEE-SERVICE.md) - Employee data
- [Notification Service](08-NOTIFICATION-SERVICE.md) - Notifications
