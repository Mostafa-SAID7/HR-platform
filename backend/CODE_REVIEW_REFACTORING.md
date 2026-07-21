# Code Review & Refactoring Plan

**Objective**: Follow SOLID principles by splitting large files into focused, single-responsibility files

**Status**: Analysis Complete | Refactoring Ready

---

## 📋 Issues Identified

### 1. Large Files with Multiple Responsibilities

| File | Size | Issues | Fix |
|------|------|--------|-----|
| `HR.Recruitment/Domain/JobPosting.cs` | 11.8 KB | 5 classes + 4 enums + 6 events | Split into 7 files |
| `HR.Performance/Domain/Performance.cs` | 9 KB | 2+ entities + logic | Split by entity |
| `HR.Audit/Domain/AuditEvent.cs` | 6.8 KB | Multiple responsibilities | Separate concerns |
| `HR.Employee/Domain/Employee.cs` | 7.8 KB | Employee + nested collections | Extract nested types |
| `HR.Payroll/Domain/Payroll.cs` | 6.4 KB | Business logic + entities | Separate domains |
| `HR.Attendance/Domain/Attendance.cs` | 6.6 KB | Multiple entity types | Split by type |
| `HR.Common/Sagas/EmployeeOnboardingSaga.cs` | 6.8 KB | Saga orchestration | Already focused (OK) |
| `HR.Audit/Application/Services/AuditEventConsumer.cs` | 10.1 KB | Consumer + processing | May need splitting |

### 2. Violations of SOLID Principles

#### Single Responsibility Principle (SRP)
- ❌ One file contains multiple domain entities
- ❌ Domain models mixed with enums and events
- ❌ Service classes doing multiple things

#### Open/Closed Principle (OCP)
- ⚠️ Adding new entity requires modifying existing files

#### Dependency Inversion (DIP)
- ✅ Generally good use of abstractions
- ⚠️ Some tight coupling in service files

---

## 🔧 Refactoring Plan

### Example: HR.Recruitment/Domain/JobPosting.cs

**Current**: 1 file with 5 classes + 4 enums + 6 events

**Target**: 9 focused files

```
HR.Recruitment/Domain/
├── JobPosting/
│   ├── JobPosting.cs                    # Aggregate root
│   ├── JobPostingStatus.cs              # Enum
│   └── Events/
│       ├── JobPostingCreatedEvent.cs
│       ├── JobPostingPublishedEvent.cs
│       └── JobPostingClosedEvent.cs
├── JobApplication/
│   ├── JobApplication.cs                # Entity
│   ├── ApplicationStatus.cs              # Enum
│   └── Events/
│       ├── ApplicationReceivedEvent.cs
│       └── ApplicationShortlistedEvent.cs
├── InterviewSchedule/
│   ├── InterviewSchedule.cs             # Entity
│   ├── InterviewStatus.cs               # Enum
│   └── Events/
│       └── InterviewScheduledEvent.cs
└── OfferLetter/
    ├── OfferLetter.cs                   # Entity
    ├── OfferStatus.cs                   # Enum
    └── Events/
        ├── OfferExtendedEvent.cs
        ├── OfferAcceptedEvent.cs
        └── OfferRejectedEvent.cs
```

---

## ✅ SOLID Principles Guide

### 1. Single Responsibility Principle (SRP)

**Rule**: One class = One reason to change

✅ **Good**:
```csharp
// JobPosting.cs - Only JobPosting aggregate logic
public class JobPosting : AggregateRoot
{
    public void Publish() { }
    public void Close() { }
}

// JobPostingStatus.cs - Only the enum
public enum JobPostingStatus
{
    Draft = 0,
    Open = 1,
    Closed = 2
}

// JobPostingPublishedEvent.cs - Only the event
public record JobPostingPublishedEvent(Guid Id, string Title) : IDomainEvent;
```

❌ **Bad**:
```csharp
// One file with EVERYTHING
public class JobPosting { }
public class JobApplication { }
public class InterviewSchedule { }
public class OfferLetter { }
public enum JobPostingStatus { }
public record JobPostingPublishedEvent { }
```

### 2. Open/Closed Principle (OCP)

**Rule**: Open for extension, closed for modification

✅ **Good**:
```csharp
// New entity can be added without modifying JobPosting
public class Candidate : BaseEntity { }
```

### 3. Liskov Substitution Principle (LSP)

**Rule**: Subtypes must be substitutable for base types

✅ **Good**:
```csharp
public abstract class DomainEntity { }
public class JobPosting : DomainEntity { }
public class JobApplication : DomainEntity { }
// Both can be used interchangeably where DomainEntity is expected
```

### 4. Interface Segregation Principle (ISP)

**Rule**: Many specific interfaces better than one general interface

✅ **Good**:
```csharp
public interface IHasStatus { JobPostingStatus Status { get; } }
public interface IApplicationWorkflow { void UpdateStatus(ApplicationStatus status); }
```

❌ **Bad**:
```csharp
public interface IAll
{
    void Publish();
    void Close();
    void UpdateStatus();
    void SetRating();
    // Too many methods
}
```

### 5. Dependency Inversion Principle (DIP)

**Rule**: Depend on abstractions, not concretions

✅ **Good**:
```csharp
public class JobPostingService
{
    private readonly IRepository<JobPosting> _repository;
    
    public JobPostingService(IRepository<JobPosting> repository)
    {
        _repository = repository; // Injected abstraction
    }
}
```

❌ **Bad**:
```csharp
public class JobPostingService
{
    private readonly JobPostingRepository _repository = new();
    // Direct instantiation - tight coupling
}
```

---

## 📁 File Organization Best Practices

### 1. Folder Structure

```
HR.Recruitment/
├── Domain/
│   ├── JobPosting/          # Aggregate root folder
│   │   ├── JobPosting.cs
│   │   ├── JobPostingStatus.cs
│   │   ├── JobPostingValidator.cs
│   │   └── Events/
│   │       ├── JobPostingCreatedEvent.cs
│   │       ├── JobPostingPublishedEvent.cs
│   │       └── JobPostingClosedEvent.cs
│   ├── JobApplication/      # Entity folder
│   │   ├── JobApplication.cs
│   │   ├── ApplicationStatus.cs
│   │   └── Events/
│   │       └── ApplicationReceivedEvent.cs
│   └── InterviewSchedule/   # Entity folder
│       └── ...
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Services/
│   └── Dtos/
├── Infrastructure/
│   └── Persistence/
└── Features/
```

### 2. Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Class | `{Name}.cs` | `JobPosting.cs` |
| Interface | `I{Name}.cs` | `IJobPostingRepository.cs` |
| Enum | `{Name}Status.cs` | `JobPostingStatus.cs` |
| Event | `{Name}Event.cs` | `JobPostingCreatedEvent.cs` |
| Command | `{Name}Command.cs` | `PublishJobPostingCommand.cs` |
| Query | `{Name}Query.cs` | `GetJobPostingsQuery.cs` |
| Handler | `{Name}Handler.cs` | `PublishJobPostingCommandHandler.cs` |
| Validator | `{Name}Validator.cs` | `PublishJobPostingCommandValidator.cs` |
| Dto | `{Name}Dto.cs` | `JobPostingDto.cs` |
| Service | `{Name}Service.cs` | `JobPostingService.cs` |
| Repository | `{Name}Repository.cs` | `JobPostingRepository.cs` |

### 3. Class File Template

One class per file (with rare exceptions):

```csharp
namespace HR.Recruitment.Domain.JobPosting;

/// <summary>
/// [Purpose of the class]
/// Responsibility: [Single responsibility]
/// </summary>
public class JobPosting : AggregateRoot
{
    // Implementation
}
```

---

## 🚀 Refactoring Steps

### Phase 1: Analysis ✅
- [x] Identify large files
- [x] Identify SRP violations
- [x] Create refactoring plan

### Phase 2: Refactoring (Next)
**Start with**:
1. `HR.Recruitment/Domain/JobPosting.cs` (11.8 KB) - Most violations
2. `HR.Performance/Domain/Performance.cs` (9 KB)
3. `HR.Audit/Domain/AuditEvent.cs` (6.8 KB)
4. `HR.Employee/Domain/Employee.cs` (7.8 KB)

**Process**:
1. Create subfolders for aggregates
2. Extract each class into its own file
3. Extract each enum into its own file
4. Extract events into Event folder
5. Update imports in all referencing files
6. Run tests to verify
7. Commit with clear message

### Phase 3: Services & Handlers
- Analyze Application/Services files
- Extract concerns if > 200 lines
- Ensure single responsibility

### Phase 4: Validation & Testing
- Run all unit tests
- Run all integration tests
- Verify build succeeds
- Code review

---

## 📊 Metrics

### Before Refactoring

| Service | Files | Avg Lines/File | Max Lines/File | Issues |
|---------|-------|----------------|----------------|--------|
| Recruitment | 8 | 245 | 565 | 5 violations |
| Performance | 10 | 180 | 430 | 3 violations |
| Employee | 12 | 195 | 350 | 2 violations |

### After Refactoring (Expected)

| Service | Files | Avg Lines/File | Max Lines/File | Issues |
|---------|-------|----------------|----------------|--------|
| Recruitment | 15+ | 120 | 250 | 0 violations |
| Performance | 18+ | 100 | 200 | 0 violations |
| Employee | 20+ | 110 | 200 | 0 violations |

---

## ✨ Benefits

✅ **Maintainability**: Easier to find and understand code  
✅ **Testability**: Smaller, focused classes easier to test  
✅ **Reusability**: Classes with single purpose can be reused  
✅ **Extensibility**: New features without modifying existing files  
✅ **Collaboration**: Team members can work on different files without conflicts  
✅ **Code Review**: Smaller changes easier to review  

---

## 📝 Example Refactoring: JobPosting Domain

### Step 1: Create Folder Structure

```bash
mkdir HR.Recruitment/Domain/JobPosting
mkdir HR.Recruitment/Domain/JobPosting/Events
mkdir HR.Recruitment/Domain/JobApplication
mkdir HR.Recruitment/Domain/JobApplication/Events
```

### Step 2: Extract JobPostingStatus.cs

```csharp
// HR.Recruitment/Domain/JobPosting/JobPostingStatus.cs
namespace HR.Recruitment.Domain.JobPosting;

public enum JobPostingStatus
{
    Draft = 0,
    Open = 1,
    Closed = 2,
    Archived = 3
}
```

### Step 3: Extract JobPosting.cs

```csharp
// HR.Recruitment/Domain/JobPosting/JobPosting.cs
namespace HR.Recruitment.Domain.JobPosting;

public class JobPosting : AggregateRoot
{
    public string Title { get; private set; }
    // ... properties
    
    public static JobPosting Create(...) { }
    public void Publish() { }
    public void Close() { }
    public void Update(...) { }
}
```

### Step 4: Extract Events

```csharp
// HR.Recruitment/Domain/JobPosting/Events/JobPostingCreatedEvent.cs
namespace HR.Recruitment.Domain.JobPosting.Events;

public record JobPostingCreatedEvent(
    Guid Id,
    string Title,
    string Department,
    List<string> RequiredSkills) : IDomainEvent;
```

### Step 5: Update Imports

Find all files referencing JobPosting and update:

```csharp
// Before
using HR.Recruitment.Domain;
var status = JobPostingStatus.Draft;

// After
using HR.Recruitment.Domain.JobPosting;
var status = JobPostingStatus.Draft;
```

---

## 🎯 Success Criteria

- ✅ No file > 300 lines (except Program.cs, DbContext)
- ✅ One class per file (rare exceptions documented)
- ✅ All unit tests passing
- ✅ All integration tests passing
- ✅ Build succeeds with 0 errors
- ✅ Code follows SOLID principles
- ✅ Clear file naming and organization
- ✅ Consistent folder structure across services

---

## 📚 References

- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Architecture Guides](https://learn.microsoft.com/en-us/dotnet/architecture/)
