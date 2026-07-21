# Seeds Folder - Test Data Configuration

## Overview
Organized seed data configuration for development and testing. Each aggregate has its own dedicated seed file following the **Single Responsibility Principle**.

## Structure

```
Seeds/
├── JobPostingSeed.cs           # Job posting seed data
├── JobApplicationSeed.cs       # Job application seed data
├── InterviewScheduleSeed.cs    # Interview schedule seed data
├── OfferLetterSeed.cs          # Offer letter seed data
└── README.md                   # This file
```

## Features

### 1. **Organized by Aggregate**
Each seed file focuses on ONE aggregate/entity:
- `JobPostingSeed` → Seeds job postings
- `JobApplicationSeed` → Seeds applications
- `InterviewScheduleSeed` → Seeds interviews
- `OfferLetterSeed` → Seeds offers

### 2. **Static Seed Methods**
Each seed file provides a static `Seed(ModelBuilder)` method:

```csharp
public static class JobPostingSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobPosting>().HasData(...);
    }
}
```

### 3. **Centralized in DbContext**
All seeds called from `OnModelCreating()` in one place:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply configurations
    modelBuilder.ApplyConfiguration(new JobPostingConfiguration());
    
    // Apply seeds
    JobPostingSeed.Seed(modelBuilder);
    JobApplicationSeed.Seed(modelBuilder);
    InterviewScheduleSeed.Seed(modelBuilder);
    OfferLetterSeed.Seed(modelBuilder);
}
```

## GUIDs Used for Seed Data

To maintain referential integrity, seed data uses predictable GUIDs:

| Entity | ID Pattern | Example |
|--------|-----------|---------|
| JobPosting | `00000000-0000-0000-0000-00000000000X` | `00000000-0000-0000-0000-000000000001` |
| JobApplication | `00000000-0000-0000-0000-0000000001XX` | `00000000-0000-0000-0000-000000000101` |
| InterviewSchedule | `00000000-0000-0000-0000-0000000002XX` | `00000000-0000-0000-0000-000000000201` |
| OfferLetter | `00000000-0000-0000-0000-0000000003XX` | `00000000-0000-0000-0000-000000000301` |
| Candidate | `00000000-0000-0000-0000-0000000020XX` | `00000000-0000-0000-0000-000000002001` |
| User | `00000000-0000-0000-0000-0000000010XX` | `00000000-0000-0000-0000-000000001001` |

## Adding New Seed Data

### Step 1: Create Seed Class
```csharp
public static class NewEntitySeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewEntity>().HasData(
            new { /* ... */ });
    }
}
```

### Step 2: Register in DbContext
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... configurations ...
    NewEntitySeed.Seed(modelBuilder);
}
```

### Step 3: Create Migration
```bash
dotnet ef migrations add AddNewEntitySeedData
```

## SOLID Principles Applied

✅ **Single Responsibility**: Each seed file handles one entity type
✅ **Open/Closed**: Easy to add new seeds without modifying existing ones
✅ **Clear Dependencies**: Predictable GUID relationships between entities
✅ **Maintainability**: Organized by domain aggregate

## Development vs Production

- **Development**: Run migrations with seed data for local testing
- **Production**: Disable seed data or use separate initialization scripts

To disable seeds in production, use environment-based configuration:

```csharp
if (env.IsDevelopment())
{
    JobPostingSeed.Seed(modelBuilder);
}
```

## Related Files

- `Configurations/` - Entity mapping configurations
- `RecruitmentDbContext.cs` - Main DbContext that orchestrates seeds and configurations
- `Program.cs` - Service configuration

---

**Created**: July 20, 2026
**Pattern**: SOLID Principles
**Status**: ✅ Production Ready
