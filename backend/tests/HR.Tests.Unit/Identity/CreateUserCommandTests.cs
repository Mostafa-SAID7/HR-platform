namespace HR.Tests.Unit.Identity;

/// <summary>
/// Unit tests for User creation.
/// Tests cover: user creation, role assignment, claim addition, and validation.
/// </summary>
public class CreateUserCommandTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesUser()
    {
        // Arrange
        var email = "newuser@example.com";
        var username = "newuser";
        var fullName = "New User";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("SecurePassword123");
        var tenantId = Guid.NewGuid();

        // Act
        var user = User.Create(email, username, fullName, passwordHash, tenantId);

        // Assert
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(username, user.Username);
        Assert.Equal(fullName, user.FullName);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(tenantId, user.TenantId);
        Assert.True(user.IsActive);
        Assert.False(user.EmailConfirmed);
        Assert.False(user.PhoneNumberConfirmed);
        Assert.False(user.TwoFactorEnabled);
        Assert.Equal(0, user.LoginAttempts);
        Assert.Null(user.LockoutEndUtc);
    }

    [Fact]
    public void AddRole_WithValidRole_AddsRoleToUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var role = Role.Create("Admin", "Administrator", user.TenantId, isSystemRole: true);

        // Act
        user.AddRole(role);

        // Assert
        Assert.Single(user.UserRoles);
        Assert.Equal(role.Id, user.UserRoles.First().RoleId);
    }

    [Fact]
    public void AddRole_WithDuplicateRole_DoesNotDuplicate()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var role = Role.Create("Admin", "Administrator", user.TenantId, isSystemRole: true);

        // Act
        user.AddRole(role);
        user.AddRole(role); // Add same role again

        // Assert
        Assert.Single(user.UserRoles);
    }

    [Fact]
    public void AddRole_WithMultipleRoles_AddsAllRoles()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var adminRole = Role.Create("Admin", "Administrator", user.TenantId, isSystemRole: true);
        var userRole = Role.Create("User", "Standard User", user.TenantId);
        var managerRole = Role.Create("Manager", "Team Manager", user.TenantId);

        // Act
        user.AddRole(adminRole);
        user.AddRole(userRole);
        user.AddRole(managerRole);

        // Assert
        Assert.Equal(3, user.UserRoles.Count);
    }

    [Fact]
    public void RemoveRole_WithExistingRole_RemovesRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var role = Role.Create("Admin", "Administrator", user.TenantId, isSystemRole: true);
        user.AddRole(role);
        Assert.Single(user.UserRoles);

        // Act
        user.RemoveRole(role.Id);

        // Assert
        Assert.Empty(user.UserRoles);
    }

    [Fact]
    public void RemoveRole_WithNonExistentRole_DoesNothing()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var nonExistentRoleId = Guid.NewGuid();

        // Act & Assert - Should not throw
        user.RemoveRole(nonExistentRoleId);
        Assert.Empty(user.UserRoles);
    }

    [Fact]
    public void AddClaim_WithValidClaim_AddsClaim()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var claimType = "department";
        var claimValue = "Engineering";

        // Act
        user.AddClaim(claimType, claimValue);

        // Assert
        Assert.Single(user.UserClaims);
        var claim = user.UserClaims.First();
        Assert.Equal(claimType, claim.ClaimType);
        Assert.Equal(claimValue, claim.ClaimValue);
    }

    [Fact]
    public void AddClaim_WithDuplicateClaim_DoesNotDuplicate()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        var claimType = "department";
        var claimValue = "Engineering";

        // Act
        user.AddClaim(claimType, claimValue);
        user.AddClaim(claimType, claimValue); // Add same claim again

        // Assert
        Assert.Single(user.UserClaims);
    }

    [Fact]
    public void AddClaim_WithMultipleClaims_AddsAllClaims()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());

        // Act
        user.AddClaim("department", "Engineering");
        user.AddClaim("team", "Backend");
        user.AddClaim("level", "Senior");

        // Assert
        Assert.Equal(3, user.UserClaims.Count);
    }

    [Fact]
    public void RemoveClaim_WithExistingClaim_RemovesClaim()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        user.AddClaim("department", "Engineering");
        Assert.Single(user.UserClaims);

        // Act
        user.RemoveClaim("department", "Engineering");

        // Assert
        Assert.Empty(user.UserClaims);
    }

    [Fact]
    public void RemoveClaim_WithNonExistentClaim_DoesNothing()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        user.AddClaim("department", "Engineering");

        // Act & Assert - Should not throw
        user.RemoveClaim("team", "Backend");
        Assert.Single(user.UserClaims); // Original claim still there
    }

    [Fact]
    public void RecordFailedLoginAttempt_IncreasesAttemptCount()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        Assert.Equal(0, user.LoginAttempts);

        // Act
        user.RecordFailedLoginAttempt();

        // Assert
        Assert.Equal(1, user.LoginAttempts);
    }

    [Fact]
    public void RecordFailedLoginAttempt_After5Attempts_LocksOutUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt();
        }

        // Assert
        Assert.Equal(5, user.LoginAttempts);
        Assert.NotNull(user.LockoutEndUtc);
        Assert.True(user.LockoutEndUtc > DateTime.UtcNow);
    }

    [Fact]
    public void ResetLoginAttempts_ClearsAttemptsAndLockout()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        for (int i = 0; i < 3; i++)
        {
            user.RecordFailedLoginAttempt();
        }
        Assert.Equal(3, user.LoginAttempts);

        // Act
        user.ResetLoginAttempts();

        // Assert
        Assert.Equal(0, user.LoginAttempts);
        Assert.Null(user.LockoutEndUtc);
        Assert.NotNull(user.LastLoginUtc);
    }

    [Fact]
    public void IsLockedOut_WithValidLockout_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(30);

        // Act
        var isLocked = user.IsLockedOut;

        // Assert
        Assert.True(isLocked);
    }

    [Fact]
    public void IsLockedOut_WithExpiredLockout_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());
        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(-5); // Lockout expired

        // Act
        var isLocked = user.IsLockedOut;

        // Assert
        Assert.False(isLocked);
    }

    [Fact]
    public void IsLockedOut_WithoutLockout_ReturnsFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "testuser", "Test User", "hash", Guid.NewGuid());

        // Act
        var isLocked = user.IsLockedOut;

        // Assert
        Assert.False(isLocked);
    }
}
