namespace HR.Employee.Features.GetEmployeeById;

using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Query to get an employee by ID.
/// </summary>
public record GetEmployeeByIdQuery(Guid EmployeeId, Guid TenantId) : IQuery<EmployeeDetailDto>;
