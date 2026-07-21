namespace HR.Identity.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Seed data configuration for Permission aggregate
/// </summary>
public static class PermissionSeed
{
    /// <summary>
    /// Seeds initial permission data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var permissions = new List<object>();

        // Employee permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Employee.Create", "Create new employees", "Employee", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Employee.Read", "View employee details", "Employee", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Employee.Update", "Update employee information", "Employee", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Employee.Delete", "Delete employee records", "Employee", "Delete", tenantId));

        // Performance permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Performance.Create", "Create performance reviews", "Performance", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Performance.Read", "View performance reviews", "Performance", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Performance.Update", "Update performance reviews", "Performance", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Performance.Delete", "Delete performance reviews", "Performance", "Delete", tenantId));

        // Payroll permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Payroll.Create", "Create payroll records", "Payroll", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Payroll.Read", "View payroll information", "Payroll", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Payroll.Update", "Update payroll records", "Payroll", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Payroll.Delete", "Delete payroll records", "Payroll", "Delete", tenantId));

        // Attendance permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Attendance.Create", "Record attendance", "Attendance", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Attendance.Read", "View attendance records", "Attendance", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Attendance.Update", "Update attendance records", "Attendance", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Attendance.Delete", "Delete attendance records", "Attendance", "Delete", tenantId));

        // Recruitment permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Recruitment.Create", "Create job postings", "Recruitment", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Recruitment.Read", "View job postings and applications", "Recruitment", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Recruitment.Update", "Update job postings", "Recruitment", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Recruitment.Delete", "Delete job postings", "Recruitment", "Delete", tenantId));

        // User management permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Users.Create", "Create new users", "Users", "Create", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Users.Read", "View user information", "Users", "Read", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Users.Update", "Update user information", "Users", "Update", tenantId));
        permissions.Add(CreatePermission(Guid.NewGuid(), "Users.Delete", "Delete users", "Users", "Delete", tenantId));

        // Role management permissions
        permissions.Add(CreatePermission(Guid.NewGuid(), "Roles.Manage", "Manage roles and permissions", "Roles", "Manage", tenantId));

        modelBuilder.Entity<Permission>().HasData(permissions.ToArray());
    }

    private static object CreatePermission(Guid id, string name, string description, string resource, string action, Guid tenantId)
    {
        return new
        {
            Id = id,
            Name = name,
            Description = description,
            Resource = resource,
            Action = action,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
    }
}
