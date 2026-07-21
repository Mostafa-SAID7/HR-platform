namespace HR.Employee.Features.UpdateEmployee;

using HR.Employee.Application.Dtos.Employee;

/// <summary>
/// Command to update an existing employee.
/// </summary>
public record UpdateEmployeeCommand(Guid EmployeeId, UpdateEmployeeRequest Request, Guid TenantId) : ICommand<EmployeeDetailDto>;
