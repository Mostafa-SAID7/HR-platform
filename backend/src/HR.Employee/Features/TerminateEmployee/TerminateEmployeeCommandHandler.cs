namespace HR.Employee.Features.TerminateEmployee;

using MediatR;
using HR.Employee.Domain.Employee;
using HR.Common.Domain.Exceptions;
using HR.Common.Application.Abstractions;

/// <summary>
/// Handler for TerminateEmployeeCommand.
/// </summary>
public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TerminateEmployeeCommandHandler> _logger;

    public TerminateEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<TerminateEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeRepository = _unitOfWork.GetRepository<Employee>();
        var employee = await employeeRepository.GetAsQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == request.TenantId, cancellationToken);

        if (employee is null)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", request.EmployeeId);
            throw new NotFoundException("Employee", request.EmployeeId);
        }

        if (employee.TerminationDate.HasValue)
        {
            _logger.LogWarning("Employee already terminated: {EmployeeId}", request.EmployeeId);
            throw new BusinessRuleViolationException("EmployeeAlreadyTerminated", "Employee has already been terminated");
        }

        // Terminate employment
        employee.Terminate(request.TerminationDate);

        employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee terminated: {EmployeeId} ({FirstName} {LastName}) on {TerminationDate}",
            request.EmployeeId, employee.FirstName, employee.LastName, request.TerminationDate);
    }
}
