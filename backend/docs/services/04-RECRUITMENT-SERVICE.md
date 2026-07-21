# Recruitment Service

**Port**: 5004 | **Status**: ✅ Phase 2 Complete | **Database**: PostgreSQL

## Overview

The Recruitment Service manages the complete hiring workflow including job postings, applications, interviews, and candidate tracking.

## Key Features

- ✅ Job posting creation and publishing
- ✅ Application management
- ✅ Interview scheduling and tracking
- ✅ Candidate pipeline management
- ✅ Job status workflow
- ✅ Application state transitions

## API Endpoints

```
POST   /api/recruitment/job-postings
GET    /api/recruitment/job-postings
POST   /api/recruitment/applications
GET    /api/recruitment/applications/{applicationId}
POST   /api/recruitment/applications/{applicationId}/advance
POST   /api/recruitment/interviews
GET    /api/recruitment/interviews/{interviewId}
POST   /api/recruitment/interviews/{interviewId}/schedule
```

## Domain Model

### JobPosting Aggregate

```csharp
public class JobPosting
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid DepartmentId { get; set; }
    public List<string> RequiredSkills { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Status { get; set; } // Draft, Published, Closed
    public List<JobApplication> Applications { get; set; }
}
```

### JobApplication Entity

```csharp
public class JobApplication
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public string CandidateName { get; set; }
    public string CandidateEmail { get; set; }
    public string ResumeUrl { get; set; }
    public string Status { get; set; } // Applied, Screening, Interview, Offer, Rejected
}
```

## Kafka Topics

| Topic | Event |
|-------|-------|
| `recruitment.job.published` | Job posted |
| `recruitment.job.closed` | Job closed |
| `recruitment.application.submitted` | App submitted |
| `recruitment.application.rejected` | App rejected |
| `recruitment.offer.made` | Offer sent |

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Recruitment/
dotnet test tests/HR.Tests.Integration/Recruitment/
```

## See Also

- [MICROSERVICES_STATUS.md](MICROSERVICES_STATUS.md)
