# Troubleshooting Guide

## Common Issues

### Database Connection Issues

**Problem**: `PostgreSQL connection refused`

**Solution**:
```bash
# Check PostgreSQL is running
psql -U postgres -c "SELECT 1"

# If not running, start it
# macOS with Homebrew
brew services start postgresql

# Windows with Docker
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15

# Verify connection string in appsettings.json
# Server=localhost;Port=5432;Database=hranalytics_dev;User Id=postgres;Password=postgres;
```

**Problem**: `Connection pool exhausted`

**Solution**:
```csharp
// Increase pool size in connection string
"Server=localhost;Max Pool Size=50;Min Pool Size=10;..."

// Or dispose connections properly
using (var context = new HrContext())
{
    // operations
}
```

### Service Won't Start

**Problem**: `Port already in use`

**Solution**:
```bash
# Find process using port 5001
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows

# Or change port in launchSettings.json
"applicationUrl": "http://localhost:5005"
```

**Problem**: `Missing dependencies`

**Solution**:
```bash
# Restore all packages
dotnet restore HRAnalytics.sln

# Clean and rebuild
dotnet clean
dotnet build --no-restore
```

### JWT/Authentication Issues

**Problem**: `401 Unauthorized on all requests`

**Solution**:
```bash
# Check JWT secret key is set
echo $JWT_SECRET_KEY

# Verify token format (3 parts separated by dots)
# Header.Payload.Signature

# Check token expiration
# Decode payload (second part) from base64
echo "eyJhbGciOi..." | base64 -d

# Verify Issuer and Audience match configuration
```

**Problem**: `Invalid token signature`

**Solution**:
```csharp
// Ensure same secret key is used for both generation and validation
// Production: Use RS256 with public/private key pair

// Check clock skew if tokens valid on server but not in tests
var validator = new JwtSecurityTokenHandler();
validator.InboundClaimTypeMap.Clear(); // Prevent claim type mapping issues
```

### Database Migration Failures

**Problem**: `Migration error: "Column XXX doesn't exist"`

**Solution**:
```bash
# Check current migration
dotnet ef migrations list --project src/HR.Employee/HR.Employee.csproj

# Rollback to previous version
dotnet ef database update PreviousMigration --project src/HR.Employee/HR.Employee.csproj

# Create new migration
dotnet ef migrations add FixColumnName --project src/HR.Employee/HR.Employee.csproj

# Apply migration
dotnet ef database update --project src/HR.Employee/HR.Employee.csproj
```

### Kafka Connection Issues

**Problem**: `Failed to connect to Kafka broker`

**Solution**:
```bash
# Check Kafka is running
docker ps | grep kafka

# Verify broker address
kafka-broker-api-versions.sh --bootstrap-server localhost:9092

# Check KAFKA_BROKERS environment variable
echo $KAFKA_BROKERS  # Should be: localhost:9092

# If using Docker, ensure services are on same network
docker network ls
```

### Performance Issues

**Problem**: `Slow query execution`

**Solution**:
```sql
-- Enable query logging
SET log_min_duration_statement = 1000;  -- Log queries > 1 second

-- Analyze query plan
EXPLAIN ANALYZE SELECT * FROM employees WHERE tenant_id = 'xxx' AND is_active = true;

-- Create missing indexes
CREATE INDEX idx_emp_tenant_active ON employees(tenant_id, is_active);
```

**Problem**: `Out of memory`

**Solution**:
```bash
# Increase application memory
export ASPNETCORE_URLS=http://localhost:5001
export DOTNET_TieredCompilation=true
export DOTNET_TieredCompilationQuickJit=true

# Or use --no-build flag to reduce memory during startup
dotnet run --no-build
```

### Test Failures

**Problem**: `Tests hang or timeout`

**Solution**:
```bash
# Run single test with verbose output
dotnet test --filter "TestName" -v n

# Increase test timeout
# In xUnit: [Fact(Timeout = 10000)]

# Check if database fixtures are properly cleaned up
# In test: await _fixture.DisposeAsync();
```

**Problem**: `Integration tests fail due to duplicate data`

**Solution**:
```csharp
// Ensure test isolation
public async Task InitializeAsync()
{
    await _fixture.ResetDatabaseAsync();
}

// Use transactions that rollback after test
using (var transaction = dbContext.Database.BeginTransaction())
{
    // test code
    await transaction.RollbackAsync();
}
```

### Docker Issues

**Problem**: `Container exits immediately`

**Solution**:
```bash
# Check logs
docker logs <container_id>

# Run with interactive terminal
docker run -it hranalytics-identity:latest

# Verify Dockerfile paths are correct
docker build -f src/HR.Identity/Dockerfile .
```

### Health Check Issues

**Problem**: `Health check endpoint returns unhealthy`

**Solution**:
```bash
# Check individual service health
curl http://localhost:5001/health -v

# Check dependencies
curl http://localhost:5001/health/ready  # Readiness
curl http://localhost:5001/health/live   # Liveness

# Inspect response details
curl http://localhost:5001/health -H "Accept: application/json"
```

## Debug Logging

Enable debug logging to troubleshoot:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Debug",
      "HR": "Debug"
    }
  }
}
```

## Getting Help

1. Check logs: `docker logs <service>`
2. Check health: `curl http://localhost:5000/health`
3. Run tests: `dotnet test`
4. Review issue tracker
5. Contact development team
