namespace HR.Employee.Features.CreateEmployee;

using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Command to create a new employee.
/// </summary>
public record CreateEmployeeCommand(CreateEmployeeRequest Request, Guid TenantId) : ICommand<EmployeeDetailDto>;
