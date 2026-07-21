namespace HR.Identity.Domain;

/// <summary>
/// User aggregate root for authentication and authorization.
/// </summary>
public class User : AggregateRoot
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? ManagerId { get; set; }

    // Relations
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Create a new user.
    /// </summary>
    public static User Create(
        string email,
        string username,
        string fullName,
        string passwordHash,
        Guid tenantId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            FullName = fullName,
            PasswordHash = passwordHash,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LoginAttempts = 0
        };
    }

    /// <summary>
    /// Add a role to the user.
    /// </summary>
    public void AddRole(Role role)
    {
        if (UserRoles.Any(ur => ur.RoleId == role.Id))
            return;

        UserRoles.Add(new UserRole { UserId = Id, RoleId = role.Id, User = this, Role = role });
    }

    /// <summary>
    /// Remove a role from the user.
    /// </summary>
    public void RemoveRole(Guid roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole is not null)
        {
            UserRoles.Remove(userRole);
        }
    }

    /// <summary>
    /// Add a claim to the user.
    /// </summary>
    public void AddClaim(string claimType, string claimValue)
    {
        if (UserClaims.Any(uc => uc.ClaimType == claimType && uc.ClaimValue == claimValue))
            return;

        UserClaims.Add(new UserClaim
        {
            Id = Guid.NewGuid(),
            UserId = Id,
            ClaimType = claimType,
            ClaimValue = claimValue,
            User = this
        });
    }

    /// <summary>
    /// Remove a claim from the user.
    /// </summary>
    public void RemoveClaim(string claimType, string claimValue)
    {
        var userClaim = UserClaims.FirstOrDefault(uc => uc.ClaimType == claimType && uc.ClaimValue == claimValue);
        if (userClaim is not null)
        {
            UserClaims.Remove(userClaim);
        }
    }

    /// <summary>
    /// Record a failed login attempt.
    /// </summary>
    public void RecordFailedLoginAttempt()
    {
        LoginAttempts++;
        if (LoginAttempts >= 5)
        {
            LockoutEndUtc = DateTime.UtcNow.AddMinutes(30);
        }
    }

    /// <summary>
    /// Reset login attempts after successful login.
    /// </summary>
    public void ResetLoginAttempts()
    {
        LoginAttempts = 0;
        LockoutEndUtc = null;
        LastLoginUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if user is locked out.
    /// </summary>
    public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc > DateTime.UtcNow;
}

/// <summary>
/// User-Role junction entity.
/// </summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
}

/// <summary>
/// User claim entity.
/// </summary>
public class UserClaim : BaseEntity
{
    public Guid UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public User? User { get; set; }
}

/// <summary>
/// Refresh token for token renewal.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDateUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedDateUtc { get; set; }
    public User? User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDateUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Create a new refresh token.
    /// </summary>
    public static RefreshToken Create(Guid userId, Guid tenantId, int expiryDays = 7)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateToken(),
            ExpiryDateUtc = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revoke the refresh token.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedDateUtc = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
