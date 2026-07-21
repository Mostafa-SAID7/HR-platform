# Payroll Service

**Port**: 5006 | **Status**: ✅ Production Ready | **Database**: PostgreSQL | **Pattern**: CQRS

## Overview

The Payroll Service handles complex salary calculations, tax computations, deduction management, and payment processing with robust state management and approval workflows.

## Key Features

- ✅ Complex payroll calculations (gross, tax, SSC, deductions)
- ✅ Tax calculation engine (10% standard rate)
- ✅ Social security contributions (5%)
- ✅ Health insurance deductions
- ✅ Approval workflow with state transitions
- ✅ Payment processing and status tracking
- ✅ Payroll reports and analytics
- ✅ Bulk payroll generation

## API Endpoints

```
POST   /api/payroll/calculate
GET    /api/payroll/{payrollId}
POST   /api/payroll/{payrollId}/approve
POST   /api/payroll/{payrollId}/process-payment
GET    /api/payroll/report
GET    /api/payroll/employee/{employeeId}
```

## Domain Model

### PayrollRecord Aggregate

```csharp
public class PayrollRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossIncome { get; set; }
    
    // Deductions
    public decimal IncomeTax { get; set; }
    public decimal SocialSecurityContribution { get; set; }
    public decimal HealthInsurance { get; set; }
    
    // Net
    public decimal NetSalary { get; set; }
    public string Status { get; set; } // Draft, Processed, Approved, Paid
    public DateTime? PaidDate { get; set; }
}
```

## Calculation Formula

```
Gross Income = Basic + Housing + Transport + Other
Tax = Gross * 10%
SSC = Gross * 5%
Health = 500 (fixed)
Net Salary = Gross - Tax - SSC - Health
```

## Database Schema

### PayrollRecords Table

| Column | Type | Description |
|--------|------|-------------|
| Id | UUID | Primary key |
| EmployeeId | UUID | Employee reference |
| EmployeeName | VARCHAR(256) | Name snapshot |
| Year | INT | Payroll year |
| Month | INT | Payroll month |
| BasicSalary | DECIMAL(18,2) | Base salary |
| HousingAllowance | DECIMAL(18,2) | Housing component |
| TransportAllowance | DECIMAL(18,2) | Transport component |
| OtherAllowances | DECIMAL(18,2) | Other allowances |
| GrossIncome | DECIMAL(18,2) | Total earnings |
| IncomeTax | DECIMAL(18,2) | 10% of gross |
| SocialSecurityContribution | DECIMAL(18,2) | 5% of gross |
| HealthInsurance | DECIMAL(18,2) | Fixed: 500 |
| NetSalary | DECIMAL(18,2) | Final amount |
| Status | VARCHAR(50) | Processing status |
| PaidDate | DATE | Payment date |
| TenantId | UUID | Tenant ID |

## Workflow

```
Draft → Calculate → Processed → Approve → Approved → Pay → Paid
```

## Kafka Topics

| Topic | Event | Data |
|-------|-------|------|
| `payroll.calculated` | Payroll calculated | PayrollId, EmployeeId, GrossIncome, NetSalary |
| `payroll.approved` | Payroll approved | PayrollId, ApprovedAt |
| `payroll.paid` | Payment processed | PayrollId, Amount, PaidAt |

## Integration Examples

### Calculate Payroll

```bash
curl -X POST http://localhost:5006/api/payroll/calculate \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": "emp-guid",
    "employeeName": "John Doe",
    "basicSalary": 100000,
    "housingAllowance": 15000,
    "transportAllowance": 5000,
    "otherAllowances": 5000,
    "year": 2026,
    "month": 7
  }'
```

### Approve Payroll

```bash
curl -X POST http://localhost:5006/api/payroll/{payrollId}/approve \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Process Payment

```bash
curl -X POST http://localhost:5006/api/payroll/{payrollId}/process-payment \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Get Payroll Report

```bash
curl "http://localhost:5006/api/payroll/report?year=2026&month=7" \
  -H "Authorization: Bearer token"
```

## Query Example

```csharp
// Get all payrolls for an employee
var payrolls = await _repository
    .GetAsQueryable()
    .Where(p => p.EmployeeId == employeeId && p.TenantId == tenantId)
    .OrderByDescending(p => p.Year)
    .ThenByDescending(p => p.Month)
    .ToListAsync();

// Generate monthly report
var report = await _repository
    .GetAsQueryable()
    .Where(p => p.Year == 2026 && p.Month == 7)
    .GroupBy(p => p.EmployeeId)
    .Select(g => new {
        TotalGross = g.Sum(p => p.GrossIncome),
        TotalNet = g.Sum(p => p.NetSalary),
        TotalTax = g.Sum(p => p.IncomeTax)
    })
    .ToListAsync();
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Payroll/
dotnet test tests/HR.Tests.Integration/Payroll/
```

## Dependencies

- Entity Framework Core 9.0
- Kafkaflow
- AutoMapper
- FluentValidation

## Related Services

- [Employee Service](02-EMPLOYEE-SERVICE.md)
- [Attendance Service](05-ATTENDANCE-SERVICE.md) - For leave deductions
- [Analytics Service](07-ANALYTICS-SERVICE.md) - For salary reports
