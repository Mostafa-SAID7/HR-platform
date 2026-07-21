namespace HR.Employee.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Employee.Domain.Department;

/// <summary>
/// Seed data configuration for Department aggregate
/// </summary>
public static class DepartmentSeed
{
    /// <summary>
    /// Seeds initial department data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var departmentId1 = Guid.Parse("00000000-0000-0000-0000-000000000101");
        var departmentId2 = Guid.Parse("00000000-0000-0000-0000-000000000102");
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Department>().HasData(
            new
            {
                Id = departmentId1,
                Name = "Engineering",
                Description = "Software Development and Engineering",
                Location = "San Francisco, CA",
                ManagerId = (Guid?)null,
                TenantId = tenantId,
                IsDeleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            },
            new
            {
                Id = departmentId2,
                Name = "Product",
                Description = "Product Management and Strategy",
                Location = "San Francisco, CA",
                ManagerId = (Guid?)null,
                TenantId = tenantId,
                IsDeleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            });
    }
}
