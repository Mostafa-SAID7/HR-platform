# Database Security & Compliance

Comprehensive guide to data security, encryption, access control, and regulatory compliance for HR Analytics Platform.

---

## Security Overview

### Compliance Requirements

| Regulation | Scope | Requirements |
|-----------|-------|--------------|
| **GDPR** | EU employees | Data encryption, right to deletion, consent tracking |
| **CCPA** | California residents | Opt-out mechanism, deletion requests, data inventory |
| **HIPAA** | If health benefits data | Encryption, access logs, breach notification |
| **SOC 2 Type II** | Enterprise customers | Audit trails, access controls, monitoring |
| **ISO 27001** | Information security | Risk management, incident response, training |

### Data Classification

```
Level 1: PUBLIC
- Company name, public articles, job listings
- No encryption needed
- Example: "HR Analytics Platform"

Level 2: INTERNAL
- Job titles, department names, general policies
- Basic encryption (in transit)
- Example: "Engineering" department

Level 3: CONFIDENTIAL
- Salary ranges, headcount, performance reviews
- Encryption at rest + in transit
- Example: Average salary by department

Level 4: RESTRICTED
- Individual salaries, SSN, health information
- Strong encryption, strict access control
- Example: "John Doe - $120,000 base salary"
```

---

## Encryption Strategy

### Data at Rest (Stored)

#### Option 1: PostgreSQL Built-in Encryption (Recommended)

```sql
-- Enable native encryption (PostgreSQL 13.2+)
-- On database creation:

CREATE DATABASE hr_analytics
  WITH OWNER hr_admin
  TEMPLATE template0
  ENCODING 'UTF8'
  LC_COLLATE 'en_US.UTF-8'
  LC_CTYPE 'en_US.UTF-8';

-- Enable SSL for connections
ALTER SYSTEM SET ssl = on;
ALTER SYSTEM SET ssl_cert_file = '/etc/ssl/postgresql/server.crt';
ALTER SYSTEM SET ssl_key_file = '/etc/ssl/postgresql/server.key';

-- Reload configuration
SELECT pg_reload_conf();
```

#### Option 2: AWS RDS with KMS Encryption

```bash
# Create RDS with AWS KMS encryption
aws rds create-db-instance \
  --db-instance-identifier hr-analytics \
  --db-instance-class db.t3.large \
  --engine postgres \
  --master-username admin \
  --allocated-storage 500 \
  --storage-encrypted \
  --kms-key-id arn:aws:kms:us-east-1:123456789:key/12345678-1234-1234-1234-123456789012 \
  --enable-cloudwatch-logs-exports postgresql \
  --backup-retention-period 30 \
  --multi-az
```

**Benefits:**
- Automatic encryption at rest
- Keys managed by AWS KMS
- Encryption transparent to application
- Hardware-backed key storage

#### Option 3: Column-Level Encryption (Sensitive Fields)

```csharp
// Encrypt specific sensitive columns in application
public class EncryptionService
{
    private readonly IDataProtectionProvider dataProtectionProvider;
    
    public EncryptionService(IDataProtectionProvider provider)
    {
        dataProtectionProvider = provider;
    }
    
    // Encrypt personal data
    public string Encrypt(string plaintext)
    {
        var protector = dataProtectionProvider.CreateProtector("HR.Sensitive");
        return protector.Protect(plaintext);
    }
    
    // Decrypt for display
    public string Decrypt(string ciphertext)
    {
        var protector = dataProtectionProvider.CreateProtector("HR.Sensitive");
        return protector.Unprotect(ciphertext);
    }
}

// Usage in model
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Encrypted in database
    [Encrypted]
    public string SocialSecurityNumber { get; set; }
    
    [Encrypted]
    public string DateOfBirth { get; set; }
    
    // In appsettings.json
    // "DataProtection": {
    //   "KeyPath": "/var/lib/aspnet/keys/"
    // }
}
```

**Encrypted Column Storage:**
```
Standard: "123-45-6789"
Encrypted: "CfDJ8EwJ1hECARDVvA2x5Gy1h0IbG7Yxzt8YHLfN9qE=="
```

### Data in Transit (Network)

#### SSL/TLS for All Connections

```csharp
// Configure PostgreSQL connection with SSL
services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(
        "Host=hr-db.amazonaws.com;" +
        "Database=hr_analytics;" +
        "Username=app_user;" +
        "Password=secure_password;" +
        "SSL Mode=Require;" +  // Enforce SSL
        "Pooling=true",
        npgsqlOptions => npgsqlOptions
            .CommandTimeout(60)
    )
);
```

**Verification:**
```bash
# Verify SSL connection
psql -h hr-db.amazonaws.com \
  -U app_user \
  -d hr_analytics \
  --set=sslmode=require \
  -c "SELECT ssl_is_used();"
```

#### API HTTPS Enforcement

```csharp
// Startup.cs - Enforce HTTPS
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHttpsRedirection();  // Always redirect HTTP → HTTPS
    app.UseHsts();              // HTTP Strict Transport Security
    
    // Additional security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        await next();
    });
}
```

---

## Access Control (Authorization)

### Role-Based Access Control (RBAC)

```sql
-- Define database roles
CREATE ROLE admin_role;
CREATE ROLE analyst_role;
CREATE ROLE viewer_role;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE hr_analytics TO admin_role;
GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA public TO analyst_role;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO viewer_role;

-- Create application users
CREATE USER app_admin WITH PASSWORD 'strong_password' IN ROLE admin_role;
CREATE USER app_analyst WITH PASSWORD 'strong_password' IN ROLE analyst_role;
CREATE USER app_viewer WITH PASSWORD 'strong_password' IN ROLE viewer_role;

-- Row-level security for multi-tenant data
ALTER TABLE employees ENABLE ROW LEVEL SECURITY;

-- Policy: analysts can only see their company's data
CREATE POLICY tenant_isolation_policy ON employees
    FOR SELECT
    USING (company_id = CURRENT_SETTING('app.current_company_id')::int);

-- Set company ID in session
SET app.current_company_id = '1';
```

### Application-Level Authorization

```csharp
// AppDbContext - Automatic RLS
public class HRContext : DbContext
{
    private readonly IHttpContextAccessor httpContextAccessor;
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<TurnoverRisk> TurnoverRisks { get; set; }
    
    public HRContext(DbContextOptions options, IHttpContextAccessor accessor)
        : base(options)
    {
        httpContextAccessor = accessor;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatic company filtering
        var companyId = httpContextAccessor.HttpContext
            ?.User.FindFirst("company_id")?.Value ?? "0";
            
        modelBuilder.Entity<Employee>()
            .HasQueryFilter(e => e.CompanyId.ToString() == companyId);
            
        modelBuilder.Entity<Department>()
            .HasQueryFilter(d => d.CompanyId.ToString() == companyId);
            
        modelBuilder.Entity<TurnoverRisk>()
            .HasQueryFilter(tr => tr.Employee.CompanyId.ToString() == companyId);
    }
}

// Secure employee access
public class EmployeeService
{
    private readonly HRContext context;
    
    // Only returns employees for current company (RLS enforced)
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await context.Employees
            .AsNoTracking()
            .ToListAsync();
    }
}
```

### API Authentication

```csharp
// Startup - Configure authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.company.com";
        options.Audience = "hr-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Get signing keys from JWKS endpoint
                var http = new HttpClient();
                var response = http.GetAsync("https://auth.company.com/.well-known/jwks.json").Result;
                var json = response.Content.ReadAsAsync<JObject>().Result;
                return json["keys"]
                    .Values<JObject>()
                    .Select(k => new JsonWebKey(k.ToString()));
            }
        };
    });

// Authorization attributes
[Authorize(Roles = "admin,analyst")]
[HttpGet("employees")]
public async Task<IActionResult> GetEmployees()
{
    var employees = await employeeService.GetEmployeesAsync();
    return Ok(employees);
}

[Authorize(Roles = "admin")]
[HttpDelete("employees/{id}")]
public async Task<IActionResult> DeleteEmployee(int id)
{
    await employeeService.DeleteEmployeeAsync(id);
    return NoContent();
}
```

---

## Audit Logging & Monitoring

### Audit Trail

```sql
-- Audit log table
CREATE TABLE audit_logs (
    id SERIAL PRIMARY KEY,
    action VARCHAR(50),
    table_name VARCHAR(100),
    record_id INT,
    old_values JSONB,
    new_values JSONB,
    changed_by VARCHAR(100),
    changed_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    company_id INT NOT NULL,
    ip_address INET,
    user_agent TEXT
);

-- Index for query performance
CREATE INDEX idx_audit_logs_date ON audit_logs(changed_date);
CREATE INDEX idx_audit_logs_company ON audit_logs(company_id);
CREATE INDEX idx_audit_logs_user ON audit_logs(changed_by);

-- Audit trigger
CREATE FUNCTION audit_trigger()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_logs (action, table_name, record_id, old_values, new_values, changed_by)
    VALUES (
        TG_OP,
        TG_TABLE_NAME,
        COALESCE(NEW.id, OLD.id),
        to_jsonb(OLD),
        to_jsonb(NEW),
        CURRENT_USER
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Enable audit on sensitive tables
CREATE TRIGGER audit_employees
AFTER INSERT OR UPDATE OR DELETE ON employees
FOR EACH ROW EXECUTE FUNCTION audit_trigger();

CREATE TRIGGER audit_compensation
AFTER INSERT OR UPDATE OR DELETE ON compensation_analysis
FOR EACH ROW EXECUTE FUNCTION audit_trigger();
```

### Application Audit Logging

```csharp
public interface IAuditService
{
    Task LogAccessAsync(string userId, string resource, string action);
    Task LogDataChangeAsync(string userId, string table, int recordId, object oldValues, object newValues);
    Task LogFailedAuthenticationAsync(string username, string reason);
}

public class AuditService : IAuditService
{
    private readonly HRContext context;
    private readonly IHttpContextAccessor httpContextAccessor;
    
    public async Task LogAccessAsync(string userId, string resource, string action)
    {
        var log = new AuditLog
        {
            Action = $"ACCESS_{action}",
            TableName = resource,
            ChangedBy = userId,
            ChangedDate = DateTime.UtcNow,
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString(),
            UserAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString()
        };
        
        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();
    }
}

// Usage
[Authorize]
[HttpGet("employees/{id}")]
public async Task<IActionResult> GetEmployee(int id)
{
    var userId = User.FindFirst("sub")?.Value;
    await auditService.LogAccessAsync(userId, "employees", $"GET_{id}");
    
    var employee = await employeeService.GetEmployeeAsync(id);
    return Ok(employee);
}
```

### Real-Time Monitoring

```yaml
# Elasticsearch alerting rules
- id: unauthorized_access_attempt
  name: "Unauthorized Database Access"
  query: |
    SELECT COUNT(*) as failed_attempts
    FROM audit_logs
    WHERE action LIKE 'AUTH_FAILED%'
    AND changed_date > NOW() - INTERVAL '5 minutes'
    GROUP BY changed_by
    HAVING COUNT(*) > 3
  threshold: 1
  notification: alert_security_team

- id: bulk_data_export
  name: "Bulk Data Export Detected"
  query: |
    SELECT * FROM audit_logs
    WHERE action = 'SELECT'
    AND table_name IN ('employees', 'compensation_analysis')
    AND (new_values::text)::length > 1000000  -- > 1MB
    AND changed_date > NOW() - INTERVAL '1 hour'
  notification: alert_security_team
```

---

## GDPR Compliance

### Required Implementation

#### 1. Data Processing Agreement (DPA)

```markdown
# Data Processing Addendum (DPA)

## Article 28 GDPR Compliance
- HR Analytics Platform processes HR data as a Data Processor
- Our customer (your company) is the Data Controller
- This DPA specifies terms for data processing

## Processing Details
- Purpose: HR analytics, predictive modeling
- Data types: Employee records, employment history, compensation
- Duration: For contract term + 30 days after
- Location: US (AWS us-east-1 region)
- Subprocessors: AWS (infrastructure), Snowflake (analytics)

## Your Rights
- Access: Audit our processing
- Correction: Update incorrect data
- Deletion: Right to be forgotten (within 30 days)
- Portability: Export your data (within 7 days)

## Our Obligations
- Data security (encryption, access control)
- Breach notification (within 72 hours)
- Assist with subject access requests
- Delete data upon termination
```

#### 2. Privacy Policy Requirements

```markdown
# Privacy Policy - What Data We Collect

## Data Collection
- Employee names, emails, phone numbers
- Employment dates, job titles, departments
- Salary, bonuses, equity information
- Performance reviews, engagement scores
- Attendance, time off records

## Legal Basis
- Legitimate Interest: HR analytics
- Contract Necessity: Service delivery
- Consent: For sensitive processing (if applicable)

## Data Subject Rights
- Access: Request copy of your data
- Rectification: Correct inaccurate data
- Erasure: Right to be forgotten
- Portability: Get data in machine-readable format
- Objection: Opt-out of processing

## Contact
privacy@company.com
Data Protection Officer: dpo@company.com
```

#### 3. Data Retention Limits

```sql
-- Enforce retention policies
DELETE FROM employees
WHERE status = 'Terminated'
  AND termination_date < CURRENT_DATE - INTERVAL '7 years';

DELETE FROM audit_logs
WHERE changed_date < CURRENT_DATE - INTERVAL '3 years';

DELETE FROM employee_events
WHERE created_date < CURRENT_DATE - INTERVAL '2 years'
  AND event_type NOT IN ('HIRE', 'PROMOTION', 'TERMINATION');

-- Schedule as nightly job
-- CRON: 0 2 * * * psql hr_analytics -c 'CALL retention_cleanup();'
```

#### 4. Data Subject Access Request (DSAR)

```csharp
public class DataSubjectAccessService
{
    private readonly HRContext context;
    private readonly IEncryptionService encryption;
    
    public async Task<DataExportDto> ExportUserDataAsync(string email)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
            
        if (employee == null)
            throw new NotFoundException("No data found for this email");
        
        // Collect all related data
        var export = new DataExportDto
        {
            EmployeeData = new
            {
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.HireDate
            },
            CompensationHistory = await context.CompensationAnalysis
                .Where(c => c.EmployeeId == employee.Id)
                .Select(c => new { c.BaseSalary, c.Bonus, c.EffectiveDate })
                .ToListAsync(),
            PerformanceReviews = await context.PerformanceReviews
                .Where(p => p.EmployeeId == employee.Id)
                .Select(p => new { p.Rating, p.Comments, p.ReviewDate })
                .ToListAsync(),
            AuditLog = await context.AuditLogs
                .Where(a => a.ChangedBy == email)
                .Select(a => new { a.Action, a.ChangedDate, a.IpAddress })
                .ToListAsync()
        };
        
        return export;
    }
    
    // Right to deletion
    public async Task DeleteUserDataAsync(string email)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
            
        if (employee == null) return;
        
        // Anonymize personal data (GDPR requirement to keep some audit trail)
        employee.FirstName = "DELETED";
        employee.LastName = "DELETED";
        employee.Email = $"deleted-{employee.Id}@deleted.local";
        employee.Phone = null;
        employee.DateOfBirth = null;
        
        // Keep audit trail but mark as deleted
        var logs = context.AuditLogs.Where(l => l.ChangedBy == email);
        foreach (var log in logs)
            log.IsGdprDeleted = true;
        
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
    }
}
```

---

## Data Breach Response

### Incident Response Plan

```
1. DETECTION (0-30 min)
   □ Identify breach scope
   □ Confirm data accessed
   □ Determine number of records
   □ Contact security team

2. CONTAINMENT (30 min - 2 hours)
   □ Revoke attacker access
   □ Reset compromised credentials
   □ Enable extra monitoring
   □ Preserve evidence

3. ASSESSMENT (2-24 hours)
   □ Evaluate risk level
   □ Determine if notification required
   □ Calculate affected individuals
   □ Document timeline

4. NOTIFICATION (24-72 hours)
   □ Notify supervisory authority (if required)
   □ Notify affected individuals
   □ Provide breach details
   □ Suggest protective measures

5. REMEDIATION (1-30 days)
   □ Implement fixes
   □ Update security measures
   □ Conduct root cause analysis
   □ Notify additional parties if needed

6. REVIEW (30-90 days)
   □ Post-incident review
   □ Update policies
   □ Security training
   □ Document lessons learned
```

### Breach Notification Template

```
Subject: Security Incident Notification - [Company Name]

Dear [Individual],

We are writing to inform you of a security incident affecting your personal data.

INCIDENT DETAILS:
- Date Discovered: [Date]
- Type of Data: [Specify - names, emails, etc.]
- Number of Records Affected: [X records]
- Date of Breach: [Date or range]

ACTIONS TAKEN:
- Breach scope limited and contained
- Unauthorized access revoked
- Enhanced monitoring enabled
- Law enforcement notified (if applicable)

YOUR RIGHTS:
- You can file a complaint with your data protection authority
- Contact privacy@company.com for more information
- Consider password reset and fraud monitoring

For questions: [contact information]

[Company Name]
[Date]
```

---

## Security Compliance Checklist

### At Rest
- [ ] Database encrypted (RDS KMS or PostgreSQL native)
- [ ] Backups encrypted (AWS S3/Glacier)
- [ ] Sensitive columns encrypted in application
- [ ] Key rotation schedule (annual minimum)

### In Transit
- [ ] TLS 1.2+ for database connections
- [ ] HTTPS enforced for API
- [ ] HSTS headers enabled
- [ ] Certificate pinning (mobile apps)

### Access Control
- [ ] Role-based access (admin, analyst, viewer)
- [ ] Row-level security (multi-tenant isolation)
- [ ] API authentication (JWT/OAuth2)
- [ ] MFA enabled for admin accounts

### Audit & Monitoring
- [ ] All database queries logged
- [ ] Failed authentication attempts logged
- [ ] Data access audited
- [ ] Real-time alerts configured
- [ ] Logs retained for 1+ year

### GDPR
- [ ] Data processing agreement signed
- [ ] Privacy policy published
- [ ] Data subject rights process documented
- [ ] Breach notification procedure ready
- [ ] Data retention policies enforced
- [ ] DSAR response time < 30 days

### Incident Response
- [ ] Response plan documented
- [ ] On-call security team identified
- [ ] Breach notification template prepared
- [ ] Law enforcement contact info available
- [ ] Insurance provider informed

---

**Last Updated:** July 2026
**Status:** Active Security Framework
**Next Audit:** January 2027
**Certification Goal:** SOC 2 Type II (Q4 2026)
