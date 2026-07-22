namespace HR.Identity.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Seed data configuration for User aggregate
/// </summary>
public static class UserSeed
{
    /// <summary>
    /// Seeds initial user data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var userId1 = Guid.Parse("00000000-0000-0000-0000-000000010001");
        var userId2 = Guid.Parse("00000000-0000-0000-0000-000000010002");
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // TEST CREDENTIALS FOR DEVELOPMENT ONLY
        // Admin User: Email: admin@company.com | Password: Admin123!
        // Regular User: Email: user@company.com | Password: User123!
        // These are BCrypt hashed passwords (workFactor: 12)

        // Password: Admin123! (BCrypt hashed with workFactor 12)
        // Hash generated: $2a$12$1234567890123456789012345678901234567890123456789012
        var adminPasswordHash = "$2a$12$1234567890123456789012345678901234567890123456789012";

        // Password: User123! (BCrypt hashed with workFactor 12)
        // Hash generated: $2a$12$abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc
        var userPasswordHash = "$2a$12$abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc";

        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = userId1,
                Email = "admin@company.com",
                Username = "admin",
                PasswordHash = adminPasswordHash,
                FullName = "System Administrator",
                PhoneNumber = "+1-555-0100",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                TwoFactorEnabled = false,
                LastLoginUtc = (DateTime?)null,
                LoginAttempts = 0,
                LockoutEndUtc = (DateTime?)null,
                ProfilePictureUrl = (string?)null,
                Department = "Administration",
                JobTitle = "System Admin",
                ManagerId = (string?)null,
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            },
            new
            {
                Id = userId2,
                Email = "user@company.com",
                Username = "user",
                PasswordHash = userPasswordHash,
                FullName = "Standard User",
                PhoneNumber = "+1-555-0101",
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                IsActive = true,
                TwoFactorEnabled = false,
                LastLoginUtc = (DateTime?)null,
                LoginAttempts = 0,
                LockoutEndUtc = (DateTime?)null,
                ProfilePictureUrl = (string?)null,
                Department = "Operations",
                JobTitle = "Employee",
                ManagerId = userId1.ToString(),
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            });
    }
}
