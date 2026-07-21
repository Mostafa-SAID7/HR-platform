namespace HR.Employee.Features.TerminateEmployee;

/// <summary>
/// Command to terminate an employee.
/// </summary>
public record TerminateEmployeeCommand(Guid EmployeeId, DateTime TerminationDate, Guid TenantId) : ICommand;
