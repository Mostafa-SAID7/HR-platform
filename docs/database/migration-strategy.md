# Database Migration Strategy & Versioning

Comprehensive guide to database migrations, schema versioning, and deployment strategies for HR Analytics Platform.

---

## Migration Philosophy

### Core Principles

1. **Zero-Downtime Deployments** - Migrations run without stopping application
2. **Backward Compatibility** - Old code must work with new schema
3. **Automatic Rollback** - Failed migrations can revert automatically
4. **Version Control** - All changes tracked in Git
5. **Testing First** - Migrations tested in staging before production

---

## Entity Framework Core Migration Strategy

### Setup EF Core Migrations

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef --version 8.0.0

# Verify installation
dotnet ef --version

# Create initial migration
cd HRAnalytics.API
dotnet ef migrations add InitialCreate --project HRAnalytics.Data
```

### Migration Folder Structure

```
HRAnalytics.Data/
├── Migrations/
│   ├── 20260701000000_InitialCreate.cs
│   ├── 20260710100000_AddTurnoverRisks.cs
│   ├── 20260715150000_AddCompensationAnalysis.cs
│   ├── 20260720120000_AddAuditLogs.cs
│   ├── 20260721080000_AddIndexes.cs
│   └── 20260725090000_AddSecurityColumns.cs
│   └── HRContextModelSnapshot.cs
└── HRContext.cs
```

### Creating Migrations

#### Safe Migration Pattern

```csharp
// Step 1: Add new column as NULLABLE first
public class AddSalaryRangeColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add nullable column (no data loss)
        migrationBuilder.AddColumn<decimal>(
            name: "salary_range_min",
            table: "employees",
            type: "numeric(10,2)",
            nullable: true);
            
        migrationBuilder.AddColumn<decimal>(
            name: "salary_range_max",
            table: "employees",
            type: "numeric(10,2)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "salary_range_min",
            table: "employees");
            
        migrationBuilder.DropColumn(
            name: "salary_range_max",
            table: "employees");
    }
}

// Step 2: Populate data with default values
public class PopulateSalaryRanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Copy existing salary to range (with buffer)
        migrationBuilder.Sql(
            @"UPDATE employees 
              SET salary_range_min = base_salary * 0.8,
                  salary_range_max = base_salary * 1.2
              WHERE base_salary IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            @"UPDATE employees 
              SET salary_range_min = NULL,
                  salary_range_max = NULL");
    }
}

// Step 3: Make column NOT NULL (after data populated)
public class MakeSalaryRangesNotNull : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "salary_range_min",
            table: "employees",
            type: "numeric(10,2)",
            nullable: false);
            
        migrationBuilder.AlterColumn<decimal>(
            name: "salary_range_max",
            table: "employees",
            type: "numeric(10,2)",
            nullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert to nullable
        migrationBuilder.AlterColumn<decimal>(
            name: "salary_range_min",
            table: "employees",
            type: "numeric(10,2)",
            nullable: true);
            
        migrationBuilder.AlterColumn<decimal>(
            name: "salary_range_max",
            table: "employees",
            type: "numeric(10,2)",
            nullable: true);
    }
}
```

### Naming Convention for Migrations

```
Format: YYYYMMDDHHMM_DescriptiveAction

Examples:
20260701_000000_InitialCreate          # Initial schema
20260710_100000_AddTurnoverRisks       # Add new table
20260715_150000_AddCompensationIndex   # Add index
20260720_120000_RenameColumn           # Schema rename
20260721_080000_AddAuditLogging        # New feature
20260725_090000_FixDataInconsistency   # Bug fix
```

---

## Deployment Strategy

### Pre-Deployment Checklist

```
Before deploying any migration:
□ Migration tested in development environment
□ Migration tested in staging environment (on production data copy)
□ Migration has both Up() and Down() methods
□ Rollback strategy documented
□ Estimated execution time known
□ Data backup created
□ Monitoring configured for issues
□ Communication plan ready
□ Team on standby for issues
```

### Deployment Steps

#### Step 1: Generate Migration Script

```bash
# Generate SQL script for review
dotnet ef migrations script --output migrations_20260725.sql \
  --idempotent \
  --configuration Release

# Review the SQL before applying
cat migrations_20260725.sql
```

#### Step 2: Review Generated SQL

```sql
-- Example generated script (idempotent - safe to run multiple times)

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] 
             WHERE [MigrationId] = N'20260725090000_AddSecurityColumns')
BEGIN
    ALTER TABLE [employees] ADD [is_gdpr_deleted] bit NOT NULL DEFAULT 0;
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725090000_AddSecurityColumns', N'8.0.0');
END;
```

#### Step 3: Deploy to Staging First

```bash
# Test migration on staging (production data replica)
dotnet ef database update --environment Staging \
  --project HRAnalytics.Data

# Run integration tests
dotnet test HRAnalytics.Tests --configuration Release

# Verify performance
# - Check query plans
# - Verify indexes created
# - Check execution times
```

#### Step 4: Deploy to Production

```bash
# Option A: Automatic deployment via CI/CD
git push origin feature/migration-xyz
# → Triggers pipeline
# → Runs tests
# → Deploys to production

# Option B: Manual deployment
dotnet ef database update --environment Production \
  --project HRAnalytics.Data

# Verify deployment
dotnet ef migrations list --environment Production
```

### Deployment Timeline

**Small Migration (< 10 seconds):**
```
09:00 - Start deployment
09:00 - Run migration
09:00 - Verify success
09:01 - Done (1 minute total downtime is acceptable)
```

**Large Migration (> 10 seconds):**
```
02:00 AM - Start (maintenance window)
02:00 - Begin migration
02:15 - Monitor progress
02:30 - Verify success
02:35 - End (off-peak hour)

Why off-peak?
- Fewer active users
- Less concurrent queries
- Easier to troubleshoot
- Lower impact if rollback needed
```

---

## Advanced Migration Patterns

### Pattern 1: Blue-Green Deployment

```
Step 1: Maintain two database copies
┌─────────────────────────┐
│ Blue DB (Current)       │  ← Production
│ Schema v1.2.3           │
└─────────────────────────┘

Step 2: Clone to Green and migrate
┌─────────────────────────┐
│ Green DB (New)          │
│ Schema v1.2.4           │  ← Run migration here
└─────────────────────────┘

Step 3: Test Green thoroughly
- Run integration tests
- Verify data integrity
- Performance test
- Load test

Step 4: Switch traffic to Green
Application connection string
FROM: Blue DB
TO: Green DB

Step 5: Keep Blue as rollback
If issues found, switch back to Blue
```

**Implementation:**

```csharp
// ConfigureServices - Connection string selection
public void ConfigureServices(IServiceCollection services)
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var connectionString = environment switch
    {
        "Production" => configuration.GetConnectionString("BlueDatabase"),  // Current
        _ => configuration.GetConnectionString("DefaultConnection")
    };
    
    services.AddDbContext<HRContext>(options =>
        options.UseNpgsql(connectionString)
    );
}

// After Green testing complete
// Update to: Green Database
```

### Pattern 2: Expand-Contract Pattern

For **renaming** columns without downtime:

```csharp
// Phase 1: Add new column (keep old)
public class ExpandPhase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add new column alongside old
        migrationBuilder.AddColumn<string>(
            name: "email_address",
            table: "employees",
            type: "text",
            nullable: true);
    }
}

// Application: Write to BOTH columns during expansion
public class EmployeeService
{
    public async Task UpdateEmployeeAsync(Employee employee)
    {
        employee.EmailAddress = employee.Email;  // Keep in sync
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
    }
}

// Phase 2: Copy data and verify
public class DataSyncPhase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Copy all data from old to new column
        migrationBuilder.Sql(
            "UPDATE employees SET email_address = email WHERE email_address IS NULL");
    }
}

// Phase 3: Contract - Remove old column
public class ContractPhase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Application now only uses email_address
        // Safe to drop old column after deployed code uses new column
        migrationBuilder.DropColumn(name: "email", table: "employees");
    }
}
```

### Pattern 3: Feature Flags with Migrations

```csharp
// Decouple migration from feature launch

public class AddNewAnalyticsColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Column exists but application doesn't use it yet
        migrationBuilder.AddColumn<int>(
            name: "new_risk_score",
            table: "employees",
            type: "integer",
            nullable: true);
    }
}

// Application: Feature flag controls usage
public class EmployeeAnalyticsService
{
    private readonly IFeatureManager featureManager;
    
    public async Task<int> GetRiskScoreAsync(int employeeId)
    {
        if (await featureManager.IsEnabledAsync("UseNewRiskScore"))
        {
            // Use new column (post-migration)
            return await context.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => e.NewRiskScore)
                .FirstOrDefaultAsync() ?? 0;
        }
        else
        {
            // Use old calculation (pre-migration)
            return await CalculateLegacyRiskScoreAsync(employeeId);
        }
    }
}

// Deployment timeline
// T-1: Deploy migration (feature flag OFF)
// T: Enable feature flag (switch to new column)
// T+1: Remove old calculation code
// T+2: Remove feature flag check
```

---

## Rollback Strategies

### Automatic Rollback on Error

```csharp
// Startup.cs - Auto-rollback on migration failure
public void Configure(IApplicationBuilder app)
{
    try
    {
        // Apply migrations at startup
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HRContext>();
            context.Database.Migrate();  // Auto-applies pending migrations
        }
    }
    catch (Exception ex)
    {
        // Log error
        logger.LogError($"Migration failed: {ex.Message}");
        
        // Option 1: Rollback to previous migration
        var context = app.ApplicationServices
            .GetRequiredService<HRContext>();
        context.Database.Migrate(
            targetMigration: "20260721080000_AddAuditLogs"  // Previous stable version
        );
        
        // Option 2: Alert team
        notificationService.AlertTeamAsync(
            "Database migration failed. Automatic rollback executed.",
            ex
        );
        
        // Option 3: Graceful degradation
        throw;  // Still throw, but gives team time to respond
    }
}
```

### Manual Rollback

```bash
# Step 1: Identify the stable migration to rollback to
dotnet ef migrations list

# Shows:
# 20260701_000000_InitialCreate
# 20260710_100000_AddTurnoverRisks
# 20260715_150000_AddCompensationAnalysis
# 20260720_120000_AddAuditLogs (← Last stable)
# 20260721_080000_AddSecurityColumns (← Failed - rollback here)

# Step 2: Generate rollback script
dotnet ef migrations script --from 20260721_080000_AddSecurityColumns \
  --to 20260720_120000_AddAuditLogs \
  --output rollback.sql \
  --idempotent

# Step 3: Review the rollback script
cat rollback.sql

# Step 4: Execute rollback
psql -h hr-db.amazonaws.com -U admin -d hr_analytics -f rollback.sql

# Step 5: Update entity model and remove failed migration
dotnet ef migrations remove  # Removes last migration
git commit -m "Revert failed migration"
```

---

## Monitoring Migrations

### Migration Validation Script

```sql
-- Run after each migration to validate

-- 1. Check for orphaned foreign keys
SELECT e.id FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
WHERE e.department_id IS NOT NULL AND d.id IS NULL;

-- 2. Check for NULL values where NOT NULL required
SELECT COUNT(*) FROM employees WHERE email IS NULL OR status IS NULL;

-- 3. Verify indexes exist
SELECT indexname FROM pg_indexes 
WHERE tablename IN ('employees', 'departments', 'compensation_analysis')
ORDER BY tablename;

-- 4. Check data integrity
SELECT COUNT(DISTINCT company_id) as companies,
       COUNT(DISTINCT department_id) as departments,
       COUNT(*) as total_employees
FROM employees;

-- 5. Performance check
EXPLAIN ANALYZE
SELECT e.id, e.name, e.salary FROM employees e
WHERE company_id = 1 AND status = 'Active'
ORDER BY salary DESC LIMIT 100;
```

### Migration Duration Monitoring

```csharp
// Middleware to track migration time
public class MigrationMonitoringMiddleware
{
    private readonly ILogger<MigrationMonitoringMiddleware> logger;
    
    public async Task InvokeAsync(HttpContext context, HRContext dbContext)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            // Attempt to apply pending migrations
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                dbContext.Database.Migrate();
            }
            
            sw.Stop();
            logger.LogInformation($"Migrations applied in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError($"Migration failed after {sw.ElapsedMilliseconds}ms: {ex}");
            throw;
        }
    }
}
```

---

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Database Migration

on:
  pull_request:
    paths:
      - 'HRAnalytics.Data/**'
  push:
    branches: [main, develop]

jobs:
  validate-migration:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: hr_analytics_test
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Generate migration script
        run: |
          dotnet ef migrations script \
            --project HRAnalytics.Data \
            --output migration.sql \
            --idempotent
      
      - name: Test migration on staging DB
        run: |
          psql -h localhost \
               -U postgres \
               -d hr_analytics_test \
               -f migration.sql
        env:
          PGPASSWORD: postgres
      
      - name: Run integration tests
        run: dotnet test HRAnalytics.Tests --configuration Release
      
      - name: Generate migration report
        run: dotnet ef migrations list --project HRAnalytics.Data
        
      - name: Comment PR with migration info
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: '✅ Migration validated successfully'
            })

  deploy-to-production:
    needs: validate-migration
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Deploy migration to production
        run: |
          dotnet ef database update \
            --project HRAnalytics.Data \
            --environment Production
        env:
          PROD_CONNECTION_STRING: ${{ secrets.PROD_DB_CONNECTION }}
      
      - name: Notify Slack on success
        uses: slackapi/slack-github-action@v1
        with:
          payload: |
            {
              "text": "✅ Database migration deployed to production"
            }
        if: success()
      
      - name: Notify Slack on failure
        uses: slackapi/slack-github-action@v1
        with:
          payload: |
            {
              "text": "❌ Database migration failed in production"
            }
        if: failure()
```

---

## Migration Changelog

### Maintain Migration Documentation

```markdown
# Migration History

## v1.2.4 - 2026-07-25
**Migration:** `20260725090000_AddSecurityColumns`
**Duration:** ~8 seconds
**Downtime:** None (NULLABLE columns added)
**Changes:**
- Added `is_gdpr_deleted` column to employees table
- Added `gdpr_deletion_date` column to employees table
- Added `ip_address` and `user_agent` to audit_logs

**Rollback:** `dotnet ef database update --target-migration 20260720120000_AddAuditLogs`
**Status:** ✅ Deployed 2026-07-25 02:30 UTC

---

## v1.2.3 - 2026-07-21
**Migration:** `20260721080000_AddAuditLogs`
**Duration:** ~45 seconds
**Downtime:** None
**Changes:**
- Created `audit_logs` table
- Added audit triggers on employees, departments
- Added indexes on created_date, changed_by

**Status:** ✅ Deployed 2026-07-21 02:00 UTC

---

## v1.2.2 - 2026-07-20
**Migration:** `20260720120000_AddAuditLogs` (partial)
**Duration:** ~2 seconds
**Status:** ✅ Deployed
```

---

## Migration Checklist

### Before Migration
- [ ] Migration script reviewed by 2+ developers
- [ ] Staging migration successful
- [ ] Data backup verified
- [ ] Rollback plan documented
- [ ] Deployment window scheduled (off-peak)
- [ ] Team notified
- [ ] Monitoring configured
- [ ] Runbook prepared

### During Migration
- [ ] Monitor application health
- [ ] Watch database logs for errors
- [ ] Check replication lag (if multi-region)
- [ ] Verify query performance post-migration
- [ ] Monitor error rates and latency

### After Migration
- [ ] Run validation queries
- [ ] Execute integration tests
- [ ] Verify indexes created
- [ ] Check slow query logs
- [ ] Update documentation
- [ ] Celebrate successful deployment

---

**Last Updated:** July 2026
**Status:** Active Migration Strategy
**Next Review:** Q4 2026
**CI/CD Platform:** GitHub Actions
