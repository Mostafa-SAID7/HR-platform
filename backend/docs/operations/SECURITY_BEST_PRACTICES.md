# Security Best Practices

## Authentication & Authorization

### JWT Token Security

- **Secret Key**: Minimum 256-bit (32 bytes) for HS256
- **Expiration**: Access tokens expire in 1 hour
- **Refresh Tokens**: Expire in 7 days, stored securely
- **Algorithm**: HS256 for development, RS256 for production

```csharp
// Generate token
var token = tokenService.GenerateAccessToken(user, roles);

// Validate token
var principal = tokenService.ValidateToken(token);
```

### Authorization Policies

Use role-based access control (RBAC):

```csharp
[Authorize(Roles = "Admin,Manager")]
public async Task ApprovePayroll(Guid payrollId)
{
    // Only Admin or Manager can approve
}
```

## Data Security

### Password Hashing

Use BCrypt with salt:

```csharp
var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
bool isValid = BCrypt.Net.BCrypt.Verify(userInput, hash);
```

### Encryption at Rest

- Encrypt sensitive fields: SSN, bank account, contact info
- Use AES-256-GCM for encryption
- Store keys in secure vault (Azure Key Vault, AWS Secrets Manager)

### Encryption in Transit

- Enforce HTTPS/TLS 1.2+
- Use certificate pinning for internal service communication
- Enable HSTS (Strict-Transport-Security)

## Input Validation

### Prevent Injection Attacks

```csharp
// ✅ Use parameterized queries
var result = dbContext.Employees.Where(e => e.Id == employeeId).FirstOrDefault();

// ❌ Avoid string concatenation
// var result = dbContext.Database.FromSqlInterpolated($"SELECT * FROM Employees WHERE Id = {employeeId}");
```

### Validate Input

```csharp
[Validate]
public class CreateUserRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }
    
    [Required, MinLength(8)]
    public string Password { get; set; }
}
```

## API Security

### Rate Limiting

Prevent brute force and DoS:

```csharp
// 100 requests per minute per client
app.UseRateLimiter();
```

### CORS Configuration

Whitelist trusted origins only:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins("https://app.example.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### API Versioning

Use URL-based versioning to track breaking changes:

```
/api/v1/employees
/api/v2/employees  // New version with breaking changes
```

## Sensitive Data Handling

### What's Sensitive?

- User credentials
- Social Security Numbers / ID numbers
- Financial information (salary, bank account)
- Personal contact information
- Health data
- Performance ratings

### Masking in Logs

```csharp
// ✅ Mask sensitive data
logger.Information("User {UserId} with email {Email} logged in", 
    userId, 
    email.Mask(3));  // Shows: j***@example.com

// ❌ Avoid logging sensitive data
// logger.Information($"Password: {password}");
```

### Audit Trail

Log all sensitive operations:

```csharp
var auditLog = new AuditLog
{
    UserId = currentUser.Id,
    Action = "ViewPayroll",
    ResourceId = payrollId,
    Timestamp = DateTime.UtcNow
};
await auditRepository.AddAsync(auditLog);
```

## Dependency Security

### Vulnerability Scanning

```bash
# Check for known vulnerabilities
dotnet list package --vulnerable

# Update packages
dotnet outdated
dotnet package update
```

### Nuget Security

- Use official NuGet.org source only
- Require package signing verification
- Pin dependency versions (no floating versions like *, >=)

## Environment Security

### Configuration Management

- Never commit secrets to Git
- Use environment variables or secret manager
- Rotate secrets regularly

```bash
# ✅ Use environment variable
export JWT_SECRET_KEY="..."

# ❌ Avoid hardcoding
var secretKey = "hardcoded-secret";
```

### HTTPS Enforcement

```csharp
app.UseHttpsRedirection();
app.UseHsts();
```

## Compliance

### Data Protection

- GDPR: Right to be forgotten, data export
- CCPA: User data transparency
- PCI DSS: If handling payment data

### Access Control

- Principle of least privilege
- Multi-factor authentication for admin accounts
- Session timeout after 30 minutes of inactivity

## Security Testing

```bash
# Run OWASP ZAP or similar
owasp-zap scan http://localhost:5000

# Penetration testing
# Test SQL injection, XSS, CSRF prevention
```

## Security Headers

```csharp
// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
```
