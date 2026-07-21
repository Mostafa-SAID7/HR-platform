namespace HR.Employee.Features.GetEmployees;

using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Query to get a list of employees with pagination and filtering.
/// </summary>
public record GetEmployeesQuery(EmployeeFilterDto Filter, Guid TenantId) : IQuery<PaginatedResult<EmployeeDto>>;
