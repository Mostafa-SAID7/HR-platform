namespace HR.Employee.Infrastructure.Persistence.Seeds;

using Microsoft.EntityFrameworkCore;
using HR.Employee.Domain.Employee;

/// <summary>
/// Seed data configuration for Employee aggregate
/// </summary>
public static class EmployeeSeed
{
    /// <summary>
    /// Seeds initial employee data for development and testing
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        var employeeId1 = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var employeeId2 = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var departmentId = Guid.Parse("00000000-0000-0000-0000-000000000101");
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Employee>().HasData(
            new
            {
                Id = employeeId1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                PhoneNumber = "1234567890",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Male",
                NationalId = "NID001",
                HireDate = new DateTime(2022, 1, 15),
                TerminationDate = (DateTime?)null,
                DepartmentId = departmentId,
                JobTitle = "Senior Software Engineer",
                EmploymentType = "Full-time",
                Salary = 120000m,
                Currency = "USD",
                Address = "123 Main St",
                City = "San Francisco",
                Country = "USA",
                PostalCode = "94105",
                ManagerId = (Guid?)null,
                Status = "Active",
                IsActive = true,
                TenantId = tenantId,
                IsDeleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            },
            new
            {
                Id = employeeId2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                PhoneNumber = "0987654321",
                DateOfBirth = new DateTime(1990, 3, 20),
                Gender = "Female",
                NationalId = "NID002",
                HireDate = new DateTime(2023, 6, 1),
                TerminationDate = (DateTime?)null,
                DepartmentId = departmentId,
                JobTitle = "Product Manager",
                EmploymentType = "Full-time",
                Salary = 110000m,
                Currency = "USD",
                Address = "456 Oak Ave",
                City = "San Francisco",
                Country = "USA",
                PostalCode = "94105",
                ManagerId = employeeId1,
                Status = "Active",
                IsActive = true,
                TenantId = tenantId,
                IsDeleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            });
    }
}
