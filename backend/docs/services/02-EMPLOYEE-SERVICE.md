# Employee Service

**Port**: 5002 | **Status**: ✅ Production Ready | **Database**: PostgreSQL | **ORM**: EF Core + Dapper

## Overview

The Employee Service manages the complete employee lifecycle including creation, updates, termination, skills management, and department assignments.

## Key Features

- ✅ CRUD operations for employees
- ✅ Employee lifecycle management (active → terminated)
- ✅ Skills tracking with proficiency levels
- ✅ Department and manager relationships
- ✅ Multi-tenancy support
- ✅ Domain events publishing
- ✅ Pagination and advanced filtering

## API Endpoints

```
POST   /api/employees
GET    /api/employees
GET    /api/employees/{employeeId}
PUT    /api/employees/{employeeId}
DELETE /api/employees/{employeeId}
POST   /api/employees/{employeeId}/skills
DELETE /api/employees/{employeeId}/skills/{skillName}
GET    /api/employees/department/{departmentId}
POST   /api/employees/{employeeId}/terminate
```

## Domain Model

### Employee Aggregate

```csharp
public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string NationalId { get; set; }
    public DateTime HireDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public string JobTitle { get; set; }
    public string EmploymentType { get; set; } // Full-time, Part-time, Contract
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public DateTime? TerminationDate { get; set; }
    public List<EmployeeSkill> Skills { get; set; }
}
```

### EmployeeSkill Entity

```csharp
public class EmployeeSkill
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string SkillName { get; set; }
    public int ProficiencyLevel { get; set; } // 1-5
    public DateTime AcquiredDate { get; set; }
}
```

## Database Schema

### Employees Table

| Column | Type | Constraint |
|--------|------|-----------|
| Id | UUID | PK |
| FirstName | VARCHAR(256) | NOT NULL |
| LastName | VARCHAR(256) | NOT NULL |
| Email | VARCHAR(256) | UNIQUE, NOT NULL |
| PhoneNumber | VARCHAR(20) | |
| DateOfBirth | DATE | |
| Gender | VARCHAR(20) | |
| NationalId | VARCHAR(50) | UNIQUE |
| HireDate | DATE | NOT NULL |
| DepartmentId | UUID | FK → Departments |
| ManagerId | UUID | FK → Employees |
| JobTitle | VARCHAR(256) | |
| EmploymentType | VARCHAR(50) | |
| Salary | DECIMAL(18,2) | |
| Status | VARCHAR(50) | Default: 'Active' |
| TerminationDate | DATE | |
| TenantId | UUID | NOT NULL |

### EmployeeSkills Table

| Column | Type |
|--------|------|
| Id | UUID |
| EmployeeId | UUID |
| SkillName | VARCHAR(256) |
| ProficiencyLevel | INT |
| AcquiredDate | DATE |

## Kafka Topics

| Topic | Event | Schema |
|-------|-------|--------|
| `employee.created` | New employee | EmployeeId, FirstName, LastName, Email, DepartmentId |
| `employee.updated` | Employee modified | EmployeeId, UpdatedFields, Timestamp |
| `employee.terminated` | Employee left | EmployeeId, TerminationDate, Reason |
| `employee.skill.added` | Skill acquired | EmployeeId, SkillName, Level |

## Integration Examples

### Create Employee

```bash
curl -X POST http://localhost:5002/api/employees \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "1234567890",
    "dateOfBirth": "1995-06-15",
    "gender": "Male",
    "hireDate": "2023-01-15",
    "departmentId": "dept-guid",
    "jobTitle": "Software Engineer",
    "employmentType": "Full-time",
    "salary": 100000
  }'
```

### Add Skill

```bash
curl -X POST http://localhost:5002/api/employees/{employeeId}/skills \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "skillName": "C#",
    "proficiencyLevel": 5
  }'
```

### Get Employees by Department

```bash
curl "http://localhost:5002/api/employees/department/{departmentId}?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer token"
```

### Terminate Employee

```bash
curl -X POST http://localhost:5002/api/employees/{employeeId}/terminate \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "terminationDate": "2026-08-31",
    "reason": "Resignation"
  }'
```

## Query Examples

### Get All Active Employees

```csharp
var employees = await _repository
    .GetAsQueryable()
    .Where(e => e.IsActive && e.TenantId == tenantId)
    .OrderBy(e => e.FirstName)
    .ToListAsync();
```

### Get Employees by Skills

```csharp
var cSharpDevelopers = await _repository
    .GetAsQueryable()
    .Where(e => e.Skills.Any(s => s.SkillName == "C#" && s.ProficiencyLevel >= 4))
    .ToListAsync();
```

## Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hr_employee;..."
  },
  "Services": {
    "EmployeeService": {
      "MaxBulkInsertSize": 1000,
      "CacheEmployeesTTL": "00:30:00"
    }
  }
}
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Employee/
dotnet test tests/HR.Tests.Integration/Employee/
```

## Dependencies

- Entity Framework Core 9.0
- Dapper (high-performance queries)
- Kafkaflow (events)
- Serilog (logging)

## Related Services

- [Identity Service](01-IDENTITY-SERVICE.md) - User management
- [Payroll Service](06-PAYROLL-SERVICE.md) - Salary calculations
- [Attendance Service](05-ATTENDANCE-SERVICE.md) - Attendance tracking
