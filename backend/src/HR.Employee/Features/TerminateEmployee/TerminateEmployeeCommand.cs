namespace HR.Employee.Features.TerminateEmployee;

/// <summary>
/// Command to terminate an employee.
/// </summary>
public record TerminateEmployeeCommand(Guid EmployeeId, DateTime TerminationDate, Guid TenantId) : ICommand;

/// <summary>
/// Validator for TerminateEmployeeCommand.
/// </summary>
public class TerminateEmployeeCommandValidator : AbstractValidator<TerminateEmployeeCommand>
{
    public TerminateEmployeeCommandValidator()
    {
        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Termination date is required")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Termination date cannot be in the past");
    }
}

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
        var employeeRepository = _unitOfWork.GetRepository<HR.Employee.Domain.Employee>();
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
