# ORM Comparison: Entity Framework Core vs Dapper

Comprehensive comparison of ORM/data access strategies for ASP.NET Core applications using Entity Framework Core and Dapper.

---

## Executive Summary

**Recommended:** Hybrid Approach
- **Entity Framework Core** (80% of code): CRUD, business logic, relationships
- **Dapper** (15% of code): Complex queries, reporting, performance-critical
- **Raw SQL** (5% of code): Bulk operations, stored procedures

**Why Hybrid:**
- EF Core: Productivity, maintainability, type safety
- Dapper: Performance, control, speed
- Best of both worlds

---

## ORM Comparison Matrix

| Aspect | EF Core | Dapper | Raw SQL | NHibernate | Linq2Db |
|--------|---------|--------|---------|-----------|---------|
| **Learning Curve** | Easy | Easy | Medium | Hard | Medium |
| **Development Speed** | Very Fast | Fast | Slow | Medium | Fast |
| **Performance** | Good | Excellent | Excellent | Good | Excellent |
| **Type Safety** | Full | Partial | None | Full | Full |
| **Relationships** | Excellent | Manual | Manual | Excellent | Good |
| **Flexibility** | Limited | High | Full | Limited | High |
| **Maintainability** | Excellent | Good | Medium | Excellent | Excellent |
| **Community** | Huge | Large | Huge | Small | Medium |
| **Learning Curve** | 2 weeks | 1 week | 1 day | 4 weeks | 2 weeks |
| **Migration Support** | Built-in | Manual | Manual | Built-in | Limited |
| **Bulk Operations** | OK | Excellent | Excellent | OK | Excellent |
| **Query Complexity** | Limited | Unlimited | Unlimited | Limited | Unlimited |
| **For HR Analytics** | 80% | 15% | 5% | ❌ | ⚠️ |

---

## Entity Framework Core (EF Core)

### What is EF Core?

Entity Framework Core is a lightweight, extensible, open-source ORM (Object-Relational Mapper) from Microsoft. It maps C# classes to database tables and handles relationships.

### Architecture

```csharp
// Domain Model (C# Class)
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    
    // Navigation property
    public Department Department { get; set; }
    public ICollection<EmployeeScore> Scores { get; set; }
}

// DbContext (Database Mapping)
public class HRContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);
    }
}

// Usage
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstOrDefaultAsync(e => e.Id == 1);
```

### Pros of Entity Framework Core

✅ **Rapid Development**
- Write classes, EF handles database
- Automatic SQL generation
- Built-in migrations
- Less code than raw SQL

✅ **Type Safety**
- Compile-time checks
- IntelliSense support
- Refactoring safe
- Fewer runtime errors

✅ **Relationships**
- Handles foreign keys
- Navigation properties
- Lazy loading, eager loading
- Include/ThenInclude

✅ **Maintainability**
- Code changes reflect on DB (migrations)
- Clean code (POCO classes)
- Easy to understand
- DDD-friendly

✅ **Built-in Features**
- Change tracking
- Unit of work pattern
- Lazy loading
- Proxy validation
- Global query filters

✅ **Query Syntax**
- Familiar LINQ syntax
- IntelliSense-friendly
- Type-safe queries
- Easy to test

✅ **Community**
- Huge ecosystem
- Lots of examples
- Microsoft-backed
- Regular updates

✅ **Migration Support**
- Schema versioning
- Automatic migration generation
- Rollback capability
- Audit trail

### Cons of Entity Framework Core

❌ **Performance**
- Slower than Dapper (20-30% overhead)
- Generated SQL sometimes suboptimal
- N+1 query problem (without Include)
- Eager/lazy loading gotchas

❌ **Complex Queries**
- Can't express some SQL easily
- Stored procedures are harder
- Custom aggregations awkward
- Window functions limited

❌ **Bulk Operations**
- Slow for bulk inserts (100k+ rows)
- No bulk updates efficiently
- Batch operations painful
- Change tracking overhead

❌ **Flexibility**
- Can't easily drop to SQL
- ORM constraints
- Less control over execution
- Harder to optimize

❌ **Learning Curve**
- Concepts (tracking, proxies, etc)
- Lazy loading pitfalls
- Query optimization
- Migration complexity

❌ **Memory**
- Tracks all loaded entities
- Can consume memory on large result sets
- No-tracking queries needed
- Proxy objects overhead

### When to Use EF Core

✅ CRUD operations
✅ Business logic with entities
✅ Simple to medium queries
✅ Relationship-heavy data
✅ Migrations important
✅ Type safety critical
✅ Teams with ORM experience

❌ Complex analytical queries
❌ Bulk operations (millions)
❌ Performance-critical code
❌ Reporting queries
❌ Ad-hoc queries

### EF Core Example: Employee CRUD

```csharp
// Dependency Injection
services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(connection));

// Create
var newEmployee = new Employee 
{ 
    Name = "John", 
    Email = "john@company.com",
    DepartmentId = 1 
};
context.Employees.Add(newEmployee);
await context.SaveChangesAsync();

// Read
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstOrDefaultAsync(e => e.Id == 1);

// Update
employee.Salary = 95000;
await context.SaveChangesAsync();

// Delete
context.Employees.Remove(employee);
await context.SaveChangesAsync();

// List with filter
var engineers = await context.Employees
    .Where(e => e.Department.Name == "Engineering")
    .OrderBy(e => e.Salary)
    .ToListAsync();

// Count
var count = await context.Employees
    .CountAsync(e => e.Department.Id == 1);
```

### Performance Characteristics

```
Operation              | Time    | Notes
---|---|---
Single Insert         | 10ms    | Fast, entity tracking
Batch Insert (100)    | 500ms   | Slow, use BulkInsert extensions
Bulk Insert (10k)     | 30s     | Very slow, consider Dapper
Simple Select         | 5ms     | Fast, optimized SQL
Select with Include   | 20ms    | Depends on relationship size
Complex Join Query    | 100ms   | May not generate optimal SQL
Bulk Update (1k+)     | 60s     | Very slow
```

---

## Dapper

### What is Dapper?

Dapper is a lightweight ORM created by Stack Overflow. It's a "micro-ORM" that maps SQL query results to C# objects with minimal overhead.

### Architecture

```csharp
// Connection + SQL Query + Parameter Mapping
var employees = connection.Query<Employee>(
    @"SELECT e.Id, e.Name, e.Email, e.Salary,
             d.Id, d.Name
      FROM Employees e
      JOIN Departments d ON e.DepartmentId = d.Id
      WHERE e.DepartmentId = @deptId
      ORDER BY e.Salary DESC",
    new { deptId = 1 }
).ToList();
```

### Pros of Dapper

✅ **Performance**
- Minimal overhead (almost like raw SQL)
- 5-10x faster than EF Core for complex queries
- Excellent for reporting
- Perfect for bulk operations

✅ **Control**
- Write exact SQL needed
- Full optimization control
- No surprises in generated SQL
- Explicit about what's happening

✅ **Flexibility**
- Any SQL possible
- Stored procedures
- Complex aggregations
- Window functions

✅ **Bulk Operations**
- Fast inserts (using BulkCopy)
- Bulk updates efficient
- Great for batch processing
- No entity tracking overhead

✅ **Low Memory**
- Lightweight mapping
- No entity tracking
- Maps results directly
- No proxy objects

✅ **Easy to Learn**
- SQL + Parameter mapping
- No ORM concepts
- Familiar to SQL developers
- Quick to start

✅ **Testing**
- Easy to mock
- SQL is explicit
- No ORM magic

### Cons of Dapper

❌ **Boilerplate**
- Write SQL manually
- Handle parameters
- Map objects yourself
- More code than EF Core

❌ **No Migrations**
- Manual schema management
- No version control for DB
- Migration planning needed
- Error-prone

❌ **No Relationships**
- No navigation properties
- Manual joining
- No lazy loading
- Load related data manually

❌ **Maintenance**
- SQL to maintain
- Schema changes = code changes
- Harder refactoring
- Less DDD support

❌ **Type Safety**
- String-based queries
- Runtime failures possible
- No compile-time checks
- Typos in column names

❌ **Discoverability**
- SQL not discoverable
- Hard to find where data used
- Query distribution unclear
- Monitoring harder

❌ **Learning Curve**
- Need SQL knowledge
- Parameter mapping concepts
- Multi-mapping

### When to Use Dapper

✅ Complex queries (multiple joins)
✅ Reporting queries
✅ Bulk operations
✅ Performance-critical code
✅ Stored procedures
✅ Analytics queries
✅ Teams with SQL skills

❌ Simple CRUD
❌ Relationship-heavy data
❌ Migrations important
❌ Rapid prototyping

### Dapper Example: Turnover Risk Query

```csharp
// Complex reporting query
var turnoverRisks = connection.Query<TurnoverRiskDto>(
    @"SELECT 
        e.Id as EmployeeId,
        e.Name,
        e.Email,
        d.Name as Department,
        m.Name as Manager,
        tr.RiskScore,
        tr.PrimaryDriver,
        tr.Engagement,
        tr.CompensationGap,
        tr.ManagerStability,
        COUNT(DISTINCT l.Id) as LeaveRequestsLast30Days
      FROM Employees e
      LEFT JOIN Departments d ON e.DepartmentId = d.Id
      LEFT JOIN Employees m ON e.ManagerId = m.Id
      LEFT JOIN TurnoverRisks tr ON e.Id = tr.EmployeeId
      LEFT JOIN LeaveRequests l ON e.Id = l.EmployeeId 
          AND l.CreatedDate > NOW() - INTERVAL '30 days'
      WHERE tr.RiskScore > @threshold
      GROUP BY e.Id, e.Name, e.Email, d.Name, m.Name, 
               tr.RiskScore, tr.PrimaryDriver, tr.Engagement, 
               tr.CompensationGap, tr.ManagerStability
      ORDER BY tr.RiskScore DESC",
    new { threshold = 0.7 }
).ToList();

// Multiple result sets
var multi = connection.QueryMultiple(
    @"SELECT * FROM Employees WHERE DepartmentId = @deptId;
      SELECT * FROM Departments WHERE Id = @deptId;",
    new { deptId = 1 }
);

var employees = multi.Read<Employee>().ToList();
var department = multi.ReadSingle<Department>();
```

### Performance Characteristics

```
Operation              | Time   | vs EF Core
---|---|---
Simple Select         | 1ms    | 5x faster
Complex Join          | 10ms   | 10x faster
Reporting Query       | 50ms   | 2-4x faster
Bulk Insert (10k)     | 1s     | 30x faster
Bulk Update (10k)     | 2s     | 30x faster
```

---

## Hybrid Strategy (RECOMMENDED)

### 80/15/5 Split

**80% - Entity Framework Core (CRUD + Business Logic)**
```csharp
// Create/Read/Update/Delete operations
var employee = await context.Employees
    .Include(e => e.Department)
    .FirstOrDefaultAsync(e => e.Id == id);

employee.Salary = newSalary;
await context.SaveChangesAsync();
```

**15% - Dapper (Complex Queries + Reporting)**
```csharp
// Reporting queries, analytics
var risks = connection.Query<RiskDto>(
    @"SELECT e.Id, e.Name, tr.RiskScore
      FROM Employees e
      JOIN TurnoverRisks tr ON e.Id = tr.EmployeeId
      WHERE tr.RiskScore > 0.7"
).ToList();
```

**5% - Raw SQL (Bulk Operations + Stored Procedures)**
```csharp
// Bulk operations
await context.Database.ExecuteSqlRawAsync(
    @"INSERT INTO EmployeeScores (EmployeeId, ScoreType, Score) 
      SELECT Id, 'TurnoverRisk', ComputedRisk FROM Employees"
);
```

### Benefits of Hybrid

✅ **Productivity:** EF Core speeds up development
✅ **Performance:** Dapper for complex/bulk operations
✅ **Maintainability:** Each tool used appropriately
✅ **Type Safety:** EF Core for business logic
✅ **Flexibility:** Dapper for edge cases
✅ **Team Skills:** Both SQL and ORM developers happy

### Implementation Pattern

```csharp
public interface IEmployeeRepository
{
    // EF Core
    Task<Employee> GetByIdAsync(int id);
    Task<List<Employee>> GetByDepartmentAsync(int deptId);
    Task SaveAsync(Employee employee);
    
    // Dapper
    Task<List<TurnoverRiskDto>> GetTurnoverRisksAsync(decimal threshold);
    Task<List<CompensationReportDto>> GetCompensationReportAsync();
    Task<int> BulkInsertScoresAsync(List<EmployeeScore> scores);
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly HRContext context;
    private readonly IDbConnection connection;
    
    // EF Core implementation
    public async Task<Employee> GetByIdAsync(int id)
    {
        return await context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    // Dapper implementation
    public async Task<List<TurnoverRiskDto>> GetTurnoverRisksAsync(decimal threshold)
    {
        return (await connection.QueryAsync<TurnoverRiskDto>(
            @"SELECT e.Id, e.Name, tr.RiskScore FROM Employees e
              JOIN TurnoverRisks tr ON e.Id = tr.EmployeeId
              WHERE tr.RiskScore > @threshold",
            new { threshold }
        )).ToList();
    }
}
```

---

## EF Core vs Dapper: Detailed Comparison

| Scenario | Winner | Why |
|----------|--------|-----|
| **CRUD (Create, Read, Update, Delete)** | EF Core | Less code, type-safe, migrations |
| **Simple Queries** | EF Core | Fast enough, maintainable |
| **Complex Multi-Join Queries** | Dapper | Better performance, explicit SQL |
| **Reporting** | Dapper | Complex aggregations, speed |
| **Bulk Insert 1M+ rows** | Dapper | 30x faster |
| **Relationship Navigation** | EF Core | Built-in support |
| **Performance-Critical Code** | Dapper | Minimal overhead |
| **Migrations & Versioning** | EF Core | Built-in system |
| **Ad-hoc Analytics** | Dapper | Flexibility |
| **Business Logic** | EF Core | Type safety, DDD |
| **Stored Procedures** | Dapper | Natural fit |
| **Team Productivity** | EF Core | Less boilerplate |
| **Code Maintainability** | EF Core | Less SQL |
| **Testing** | EF Core | InMemory DB |
| **Learning Curve** | Dapper | Simpler (just SQL) |

---

## Alternative ORMs (Brief)

### LinqToDB
- ✅ High performance
- ✅ Type-safe LINQ
- ❌ Smaller community
- ❌ Less mature

### NHibernate
- ✅ Full-featured
- ✅ Heavy JPA support
- ❌ Complex setup
- ❌ Steep learning curve
- ❌ Active development slow

### Massive
- ✅ Lightweight
- ❌ Minimal features
- ❌ Small community

**Recommendation:** Stick with EF Core + Dapper.

---

## Decision Tree

```
START: Need data access?
│
├─ Simple CRUD/relationships?
│  └─ YES → Use EF Core ✅
│
├─ Complex query/reporting?
│  └─ YES → Use Dapper ✅
│
├─ Bulk operation (1M+ rows)?
│  └─ YES → Use Dapper ✅
│
├─ Need migrations/versioning?
│  └─ YES → Use EF Core ✅
│
├─ Performance critical?
│  └─ YES → Use Dapper ✅
│
└─ Business logic with entities?
   └─ YES → Use EF Core ✅
```

---

## Configuration Example

```csharp
// Program.cs - Dependency Injection Setup
services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.CommandTimeout(60)
    )
);

// Register Dapper connections
services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))
);

// Repository layer
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Usage in controller
[ApiController]
public class EmployeeController
{
    private readonly IEmployeeRepository employeeRepo;
    
    public EmployeeController(IEmployeeRepository employeeRepo)
    {
        this.employeeRepo = employeeRepo;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        // Uses EF Core
        var employee = await employeeRepo.GetByIdAsync(id);
        return Ok(employee);
    }
    
    [HttpGet("risks")]
    public async Task<ActionResult<List<TurnoverRiskDto>>> GetTurnoverRisks()
    {
        // Uses Dapper
        var risks = await employeeRepo.GetTurnoverRisksAsync(0.7m);
        return Ok(risks);
    }
}
```

---

## Migration Strategy (EF Core)

```csharp
// Create migration after model change
// dotnet ef migrations add AddTurnoverRiskTable

public partial class AddTurnoverRiskTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TurnoverRisks",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", 
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EmployeeId = table.Column<int>(nullable: false),
                RiskScore = table.Column<decimal>(nullable: false),
                PrimaryDriver = table.Column<string>(nullable: true),
                CreatedDate = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TurnoverRisks", x => x.Id);
                table.ForeignKey(
                    name: "FK_TurnoverRisks_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TurnoverRisks");
    }
}

// Apply migration
// dotnet ef database update
```

---

## Final Recommendation for HR Analytics Platform

**Use Hybrid Approach:**

1. **Entity Framework Core (80%)**
   - Employee, Department, User, Company entities
   - CRUD operations
   - Business logic
   - Relationships & navigation

2. **Dapper (15%)**
   - Turnover risk reporting queries
   - Compensation analysis
   - Hiring analytics
   - Bulk score updates

3. **Raw SQL (5%)**
   - Bulk insert/update scripts
   - Stored procedures for complex calculations
   - Legacy integration

**Result:**
- ✅ Fast development (EF Core)
- ✅ High performance (Dapper for analytics)
- ✅ Type safety where it matters
- ✅ Flexibility where needed
- ✅ Easy to maintain

---

**Last Updated:** July 2026
**Status:** Active ORM Strategy
**Recommendation:** Hybrid EF Core (80%) + Dapper (15%) + Raw SQL (5%)
