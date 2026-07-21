# Identity Service

**Port**: 5001 | **Status**: ✅ Production Ready | **Language**: C# | **Framework**: ASP.NET Core 9

## Overview

The Identity Service is the authentication and authorization backbone of the HR Analytics Platform. It manages user authentication, JWT token generation, role-based access control (RBAC), and OAuth2 integration.

## Key Features

- ✅ JWT-based authentication
- ✅ Role-Based Access Control (RBAC)
- ✅ OAuth2 / OpenID Connect support
- ✅ Account lockout after 5 failed login attempts
- ✅ Token refresh mechanism (7-day validity)
- ✅ Claim-based authorization
- ✅ Multi-tenancy support

## API Endpoints

### Authentication

```
POST   /api/identity/login
POST   /api/identity/refresh-token
POST   /api/identity/logout
POST   /api/identity/register
GET    /api/identity/me
POST   /api/identity/change-password
```

### Role Management

```
POST   /api/identity/roles
GET    /api/identity/roles
PUT    /api/identity/roles/{roleId}
DELETE /api/identity/roles/{roleId}
```

### User Management

```
POST   /api/identity/users
GET    /api/identity/users
GET    /api/identity/users/{userId}
PUT    /api/identity/users/{userId}
DELETE /api/identity/users/{userId}
```

## Domain Model

### User Aggregate

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public List<UserRole> UserRoles { get; set; }
    public List<UserClaim> UserClaims { get; set; }
}
```

### RefreshToken Entity

```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDateUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDateUtc { get; set; }
}
```

## Database Schema

### Users Table

| Column | Type | Description |
|--------|------|-------------|
| Id | UUID | Primary key |
| Email | VARCHAR(256) | Unique email |
| Username | VARCHAR(256) | Unique username |
| FullName | VARCHAR(256) | Full name |
| PasswordHash | VARCHAR(MAX) | Hashed password (BCrypt) |
| IsActive | BOOLEAN | Account status |
| EmailConfirmed | BOOLEAN | Email verification |
| TwoFactorEnabled | BOOLEAN | 2FA status |
| LoginAttempts | INT | Failed login count |
| LockoutEndUtc | TIMESTAMP | Lockout expiration |
| LastLoginUtc | TIMESTAMP | Last login time |
| CreatedOnUtc | TIMESTAMP | Creation timestamp |
| TenantId | UUID | Tenant identifier |

### RefreshTokens Table

| Column | Type | Description |
|--------|------|-------------|
| Id | UUID | Primary key |
| UserId | UUID | User reference |
| Token | VARCHAR(MAX) | Refresh token (Base64) |
| ExpiryDateUtc | TIMESTAMP | Token expiration |
| IsRevoked | BOOLEAN | Revocation status |
| RevokedDateUtc | TIMESTAMP | Revocation date |

## Configuration

### appsettings.json

```json
{
  "JwtOptions": {
    "SecretKey": "your-256-bit-secret-key-here",
    "Issuer": "https://identityservice",
    "Audience": "hranalytics",
    "AccessTokenExpirationSeconds": 3600,
    "RefreshTokenExpirationDays": 7
  },
  "OAuth2": {
    "GoogleClientId": "xxx.apps.googleusercontent.com",
    "GoogleClientSecret": "client_secret",
    "MicrosoftClientId": "yyy",
    "MicrosoftClientSecret": "secret"
  }
}
```

## Kafka Topics

| Topic | Event | Schema |
|-------|-------|--------|
| `identity.user.created` | User registration | UserId, Email, FullName, CreatedAt |
| `identity.user.login` | Login occurred | UserId, Email, LoginTime, IpAddress |
| `identity.password.changed` | Password changed | UserId, ChangedAt |
| `identity.lockout` | Account locked | UserId, LockoutUntil |

## Integration Examples

### Login Flow

```bash
curl -X POST http://localhost:5001/api/identity/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123"
  }'

# Response
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64-encoded-token",
  "expiresIn": 3600,
  "email": "user@example.com",
  "fullName": "John Doe",
  "roles": ["Admin", "Manager"]
}
```

### Refresh Token

```bash
curl -X POST http://localhost:5001/api/identity/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "base64-encoded-token"
  }'
```

### Create User (Admin Only)

```bash
curl -X POST http://localhost:5001/api/identity/users \
  -H "Authorization: Bearer eyJhbGc..." \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "username": "newuser",
    "fullName": "New User",
    "password": "InitialPassword123",
    "roles": ["User"]
  }'
```

## Health Check

```bash
curl http://localhost:5001/health
# Response
{
  "status": "Healthy",
  "timestamp": "2026-07-21T10:30:00Z",
  "database": "Connected",
  "redis": "Connected"
}
```

## Security Notes

- Passwords are hashed using BCrypt (salt rounds: 12)
- JWT tokens are signed with HS256
- Refresh tokens are stored as Base64-encoded 256-bit random values
- Account lockout: 5 failed attempts → 30-minute lockout
- Token expiration: 1 hour (access), 7 days (refresh)
- All passwords updated on first login

## Testing

Run unit and integration tests:

```bash
dotnet test tests/HR.Tests.Unit/Identity/
dotnet test tests/HR.Tests.Integration/Identity/
```

## Dependencies

- Entity Framework Core 9.0
- JWT Bearer Authentication
- BCrypt.Net-Next
- Serilog (logging)
- Kafkaflow (event publishing)

## See Also

- [SECURITY_BEST_PRACTICES.md](../operations/SECURITY_BEST_PRACTICES.md)
- [DEPLOYMENT_GUIDE.md](../operations/DEPLOYMENT_GUIDE.md)
- [OAuth2 Integration](../../README.md)
