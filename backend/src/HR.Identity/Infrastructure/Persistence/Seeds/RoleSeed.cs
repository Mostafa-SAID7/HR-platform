namespace HR.Identity.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Seed data configuration for Role aggregate
/// </summary>
public static class RoleSeed
{
    /// <summary>
    /// Seeds initial role data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000020001");
        var userRoleId = Guid.Parse("00000000-0000-0000-0000-000000020002");
        var managerRoleId = Guid.Parse("00000000-0000-0000-0000-000000020003");
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Role>().HasData(
            new
            {
                Id = adminRoleId,
                Name = "Administrator",
                Description = "System administrator with full access to all features",
                IsSystemRole = true,
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            },
            new
            {
                Id = userRoleId,
                Name = "User",
                Description = "Standard user with limited access to features",
                IsSystemRole = true,
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            },
            new
            {
                Id = managerRoleId,
                Name = "Manager",
                Description = "Manager with access to manage subordinates and reports",
                IsSystemRole = true,
                TenantId = tenantId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            });
    }
}
